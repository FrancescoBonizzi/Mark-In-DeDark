import { Application, Point, Rectangle, Sprite, Text, TextStyle, TextureSource } from "pixi.js";
import { getTexture, loadAssets } from "../assets/AssetsLoader";
import { Enemy } from "../characters/Enemy";
import { Player } from "../characters/Player";
import { createEnemiesFromGrid } from "../characters/enemies/EnemyFactory";
import { Goblin } from "../characters/enemies/Goblin";
import { CELL_SIZE } from "../grid/GameCell";
import { GameGrid, GRID_MAX_HEIGHT } from "../grid/GameGrid";
import { loadAndParseLevel } from "../grid/GridLoader";
import { createLevelConfig } from "../grid/LevelConfig";
import { createLevelCustomization } from "../grid/LevelCustomization";
import { GroundTypes } from "../grid/enums";
import Controller from "../interaction/Controller";
import { MarkLight } from "../lighting/MarkLight";
import { ShadowSystem } from "../lighting/ShadowSystem";
import { BrazierLit } from "../objects/BrazierLit";
import { CandlePack } from "../objects/CandlePack";
import { CandlesManager } from "../objects/CandlesManager";
import { Exit } from "../objects/Exit";
import { GoblinTriggerTrap } from "../objects/GoblinTriggerTrap";
import { InstantlyDieTrap } from "../objects/InstantlyDieTrap";
import { Key } from "../objects/Key";
import { TimedBreakableTrap } from "../objects/TimedBreakableTrap";
import { TorchFireParticleSystem } from "../particleEmitters/TorchFireParticleSystem";
import Numbers from "../services/Numbers";
import { hasPhysicalKeyboard } from "../services/DeviceGuard";
import { SoundManagerInstance } from "../services/SoundInstance";
import { GameHud } from "../ui/GameHud";
import { GameTexts } from "../ui/GameTexts";
import Camera from "../world/Camera";

// Task 1 triage: warn early when runtime lighting prerequisites are missing.

const GAME_W = 1280;
const GAME_H = 720;
const GAMEOVER_KEY = "mark-in-dedark-gameover";

let app: Application | null = null;
let gameContainer: HTMLDivElement | null = null;
let orientationOverlay: HTMLDivElement | null = null;
let resizeObserver: ResizeObserver | null = null;
let camera: Camera | null = null;
let grid: GameGrid | null = null;
let shadowSystem: ShadowSystem | null = null;
let controller: Controller | null = null;
let player: Player | null = null;
let candlesManager: CandlesManager | null = null;
let candlePacks: CandlePack[] = [];
let key: Key | null = null;
let exit: Exit | null = null;
let traps: (InstantlyDieTrap | TimedBreakableTrap | GoblinTriggerTrap)[] = [];
let braziers: BrazierLit[] = [];
let enemies: Enemy[] = [];
let hud: GameHud | null = null;
let elapsedMs = 0;
let gameEnded = false;
let playerDyingDetected = false;
let deathOverlaySprite: Sprite | null = null;
let triggeredCells = new Set<object>();
let lockedExitNotified = false;
let keyOverlayText: Text | null = null;
let keyOverlayTimer = 0;
let currentLevelId = 1;
let currentFolder = "";
let visibilityHandler: (() => void) | null = null;
let torchFireParticles: TorchFireParticleSystem | null = null;


function navigateToGameOver(): void {
  // Avoids circular import with router.ts — Navigo listens to hashchange
  window.location.hash = "#/gameover";
}

export async function initGame(
  container: HTMLElement,
  folder: string,
  levelIdParam: string,
) {
  console.log(
    "[gamebootstrap] initGame called, folder=" +
      folder +
      ", level=" +
      levelIdParam +
      ", hasKeyboard=" +
      hasPhysicalKeyboard(),
  );
  if (app) {
    destroyGame();
  }

  currentFolder = folder;
  currentLevelId = parseInt(levelIdParam, 10) || 1;
  elapsedMs = 0;
  gameEnded = false;
  playerDyingDetected = false;
  deathOverlaySprite = null;
  triggeredCells = new Set();
  lockedExitNotified = false;
  keyOverlayText = null;
  keyOverlayTimer = 0;

  if (!hasPhysicalKeyboard()) {
    container.innerHTML = `
        <div id="keyboard-overlay">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="64" height="64"
               fill="none" stroke="white" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <rect x="2" y="4" width="20" height="14" rx="2" />
            <line x1="6" y1="8" x2="6" y2="8" />
            <line x1="10" y1="8" x2="10" y2="8" />
            <line x1="14" y1="8" x2="14" y2="8" />
            <line x1="18" y1="8" x2="18" y2="8" />
            <line x1="6" y1="12" x2="6" y2="12" />
            <line x1="18" y1="12" x2="18" y2="12" />
            <line x1="8" y1="16" x2="16" y2="16" />
          </svg>
          <p>Questo gioco richiede una tastiera</p>
        </div>
        `;
    return;
  }

  container.innerHTML = `
    <div id="game-wrapper">
        <div id="game-container"></div>
        <div id="orientation-overlay">
          <svg class="rotate-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
               fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 12a9 9 0 1 1-9-9c2.52 0 4.93 1 6.74 2.74L21 8" />
            <path d="M21 3v5h-5" />
          </svg>
          <p>Ruota il dispositivo</p>
        </div>
    </div>
    `;

  gameContainer = container.querySelector<HTMLDivElement>("#game-container");
  if (!gameContainer) {
    console.error("Game container non trovato");
    return;
  }

  orientationOverlay = container.querySelector<HTMLDivElement>(
    "#orientation-overlay",
  );

  app = new Application();

  TextureSource.defaultOptions.scaleMode = "nearest";

  await app.init({
    background: "#0a0a0a",
    width: GAME_W,
    height: GAME_H,
    premultipliedAlpha: false,
    antialias: true,
    autoDensity: true,
    resolution: Math.min(window.devicePixelRatio || 1, 2),
  });

  try {
    await (
      screen.orientation as unknown as { lock: (o: string) => Promise<void> }
    ).lock("landscape");
  } catch {
    /* not supported on all browsers */
  }

  resizeObserver = new ResizeObserver(resize);
  resizeObserver.observe(gameContainer);
  resize();

  gameContainer.appendChild(app.canvas);

  try {
    const [assets, bundle] = await Promise.all([
      loadAssets(),
      loadAndParseLevel(currentFolder, currentLevelId),
    ]);

    const levelConfig = createLevelConfig(
      bundle.envTexturePackId,
      assets.spriteSheets,
    );
    const levelCustomization = createLevelCustomization(currentLevelId);

    camera = new Camera(app, GRID_MAX_HEIGHT);
    grid = new GameGrid(bundle, levelConfig, levelCustomization);

    // Close all interactive doors after grid initialization
    grid.closeAllDoors();

    const glowTexture = (() => {
      try {
        return getTexture(assets.spriteSheets, "Effects/glow");
      } catch {
        return null;
      }
    })();
    if (!glowTexture) {
      console.warn(
        "[gamebootstrap] Effects/glow texture missing: glow-based lighting passes will be skipped.",
      );
    }

    shadowSystem = new ShadowSystem(
      app,
      camera,
      grid.groundContainer,
      grid.groundNormalContainer,
      grid.groundSpecularContainer,
      grid.objectsContainer,
      grid.objectsNormalContainer,
      glowTexture,
    );
    shadowSystem.ambientColor = levelCustomization.penumbraAmbientColor;
    shadowSystem.ambientAlpha = levelCustomization.penumbraAmbientAlpha;
    shadowSystem.normalMapAmbienceColor =
      levelCustomization.normalMapAmbienceColor;
    shadowSystem.glowScale = levelCustomization.glowScale;
    shadowSystem.addHullsFromWalls(grid.allWalls);
    shadowSystem.addHullsFromPillars(grid.allPillars);

    // Create player and controller
    controller = new Controller();
    player = new Player(
      grid,
      assets.spriteSheets,
      grid.objectsContainer,
      grid.objectsNormalContainer,
      shadowSystem.shadowContainer,
    );

    if (player.surroundingLight) {
      shadowSystem.addLight(player.surroundingLight);
    }

    // CandlesManager: start with 0 — candles come from CandlePacks placed in the level (C# behavior)
    candlesManager = new CandlesManager(
      assets.spriteSheets,
      grid.objectsContainer,
      shadowSystem,
      0,
    );
    player.setCandlesManager(candlesManager);
    player.onMessage = (text, color) => hud?.showMessage(text, color);

    // Torch fire particle effect (port of C# CandleFireParticleSystem on player)
    torchFireParticles = new TorchFireParticleSystem(
      getTexture(assets.spriteSheets, "Effects/particle"),
      camera,
    );

    // CandlePacks from grid placements
    candlePacks = grid.candlePlacements.map(
      (p) =>
        new CandlePack(
          assets.spriteSheets,
          p.col * CELL_SIZE + CELL_SIZE / 2,
          p.row * CELL_SIZE + CELL_SIZE / 2,
          p.count,
          grid!.objectsContainer,
        ),
    );

    // Braziers
    braziers = grid.brazierPlacements.map((p) => {
      const b = new BrazierLit(
        assets.spriteSheets,
        p.col * CELL_SIZE + CELL_SIZE / 2,
        p.row * CELL_SIZE + CELL_SIZE / 2,
        grid!.objectsContainer,
        grid!.objectsNormalContainer,
        shadowSystem!.shadowContainer,
      );
      shadowSystem!.addLight(b.surroundingLight);
      return b;
    });

    // Key
    key = new Key(
      assets.spriteSheets,
      grid.keyCol * CELL_SIZE + CELL_SIZE / 2,
      grid.keyRow * CELL_SIZE + CELL_SIZE / 2,
      grid.objectsContainer,
    );
    shadowSystem.addLight(key.surroundingLight);

    // Exit
    exit = new Exit(
      grid.exitCol * CELL_SIZE + CELL_SIZE / 2,
      grid.exitRow * CELL_SIZE + CELL_SIZE / 2,
      grid.exitDoor,
      grid,
    );
    shadowSystem.addLight(exit.surroundingLight);

    // Lucchetto sopra la porta di uscita (visibile da subito)
    {
      const lockText = new Text({
        text: '🔒',
        style: new TextStyle({ fontSize: 48 }),
      });
      lockText.anchor.set(0.5);
      lockText.x = exit.centerX;
      lockText.y = exit.centerY - 80;
      lockText.zIndex = 9000;
      grid.objectsContainer.addChild(lockText);
      keyOverlayText = lockText;
    }

    // Entrance — static ambient light on the spawn door (no game logic, purely visual)
    {
      const cx = grid.playerStartCol * CELL_SIZE + CELL_SIZE / 2;
      const cy = grid.playerStartRow * CELL_SIZE + CELL_SIZE / 2;
      const entranceLight = new MarkLight({
        x: cx,
        y: cy,
        z: 31,
        opacity: 0.3,
        radius: 80,
        castsShadows: false,
        color1: 0x55ddcc,
        color2: 0x33aaaa,
        distanceSquared: 2_200,
        distanceSquaredForWall: 7_000,
      });
      entranceLight.lightMaskScale = 0.9;
      entranceLight.deltaForWall = { x: 0, y: -110 };
      entranceLight.deltaForGround = { x: 0, y: -20 };
      shadowSystem.addLight(entranceLight);
    }

    // Traps
    traps = grid.trapPlacements.map((t) => {
      const cx = t.col * CELL_SIZE;
      const cy = t.row * CELL_SIZE;
      if (t.groundType === GroundTypes.TrappolaMorteIstantanea) {
        return new InstantlyDieTrap(
          assets.spriteSheets,
          cx,
          cy,
          CELL_SIZE,
          grid!.groundContainer,
        );
      } else if (t.groundType === GroundTypes.TrappolaTemporizzata) {
        return new TimedBreakableTrap(
          assets.spriteSheets,
          cx,
          cy,
          CELL_SIZE,
          grid!.groundContainer,
          () => camera?.startShake(1),
        );
      } else {
        // TriggerGoblin — spawns a Goblin on trigger
        const triggerCol = t.col;
        const triggerRow = t.row;
        return new GoblinTriggerTrap(
          assets.spriteSheets,
          cx,
          cy,
          CELL_SIZE,
          grid!.groundContainer,
          () => {
            if (!grid || !shadowSystem || !player || !candlesManager) return;
            const cell = grid.getCell(triggerCol, triggerRow);
            if (!cell) return;
            const goblin = new Goblin(
              grid,
              assets.spriteSheets,
              grid.objectsContainer,
              grid.objectsNormalContainer,
              cell.centerX,
              cell.centerY,
              player,
              candlesManager,
            );
            goblin.onSpeedWarning = () => hud?.showMessage(GameTexts.goblinApproaching, '#ff6600');
            if (goblin.surroundingLight)
              shadowSystem.addLight(goblin.surroundingLight);
            enemies.push(goblin);
          },
        );
      }
    });

    // Blood decal texture
    const bloodTexture = getTexture(assets.spriteSheets, "Effects/blood_decal");
    grid.setBloodDecalTexture(bloodTexture);

    // Create enemies from grid placements
    const onShake = (intensity: number) => camera?.startShake(intensity);
    enemies = createEnemiesFromGrid(
      grid,
      assets.spriteSheets,
      grid.objectsContainer,
      grid.objectsNormalContainer,
      player,
      candlesManager,
      shadowSystem,
      onShake,
    );
    for (const enemy of enemies) {
      enemy.onDeath = (x, y) => grid!.applyBloodDecal(x, y);
      if (enemy instanceof Goblin) {
        enemy.onSpeedWarning = () => hud?.showMessage(GameTexts.goblinApproaching, '#ff6600');
      }
    }
    if (shadowSystem.lights.length === 0) {
      console.warn(
        "[gamebootstrap] no lights registered after initialization; frame will appear black.",
      );
    }

    // HUD
    const gameWrapper =
      container.querySelector<HTMLDivElement>("#game-wrapper");
    if (gameWrapper) {
      hud = new GameHud(gameWrapper, bundle.levelName);
    }

    // Center camera on player start
    const zoom = camera.getZoom();
    const startX = grid.playerStartCol * CELL_SIZE + CELL_SIZE / 2;
    const startY = grid.playerStartRow * CELL_SIZE + CELL_SIZE / 2;
    camera.x = startX - GAME_W / 2 / zoom;
    camera.y = startY + GAME_H / 2 / zoom;

    app.ticker.add((ticker) => {
      if (!camera || !grid || !player || !controller) return;
      const dt = ticker.deltaMS;

      // After game ends: keep rendering animations and popups until navigation
      if (gameEnded) {
        hud?.updatePopups(dt);
        player.update(dt);
        for (const enemy of enemies) enemy.update(dt);
        if (shadowSystem) shadowSystem.update(app!, dt);
        // Sync death overlay sprite texture with the ongoing death animation
        if (deathOverlaySprite) {
          deathOverlaySprite.texture = player.mainSprite.texture;
          deathOverlaySprite.scale.x = player.mainSprite.scale.x;
        }
        return;
      }

      elapsedMs += dt;
      hud?.update(candlesManager?.leftCandles ?? 0, elapsedMs, key?.isCollected ?? false);
      hud?.updatePopups(dt);

      // Input + player update
      player.handleInput(controller.currentAction);
      player.update(dt);

      // Enemies
      for (const enemy of enemies) {
        enemy.update(dt);
      }

      // CandlesManager
      candlesManager?.update(dt);

      // Collectables
      if (key && !key.isCollected) {
        key.update(dt);
        const playerRect = player.collisionRectangle;
        if (intersects(playerRect, key.collisionRectangle)) {
          key.collect();
          hud?.showMessage(GameTexts.keyFound, "#ffd700");
          if (keyOverlayText) keyOverlayText.text = '🔓';
        }
      } else {
        key?.update(dt);
      }

      for (let i = candlePacks.length - 1; i >= 0; i--) {
        const pack = candlePacks[i]!;
        pack.update(dt);
        if (
          !pack.isCollected &&
          intersects(player.collisionRectangle, pack.collisionRectangle)
        ) {
          pack.collect();
          candlesManager?.addCandles(pack.count);
          hud?.showMessage(GameTexts.candlePackFound(pack.count), "#ffa500");
        }
        if (pack.isGone) candlePacks.splice(i, 1);
      }

      // Exit collision — locked feedback or win
      if (exit && !player.isDead) {
        if (intersects(player.collisionRectangle, exit.collisionRectangle)) {
          // B1: porta si apre solo quando il player arriva con la chiave
          if (!exit.isOpen && key?.isCollected) {
            exit.setOpen();
            // B2: rimuovi il lucchetto sopra la porta
            if (keyOverlayText) {
              keyOverlayText.parent?.removeChild(keyOverlayText);
              keyOverlayText.destroy();
              keyOverlayText = null;
            }
          }
          if (exit.isOpen) {
            gameEnded = true;
            player.win(exit.centerX, exit.centerY);
            hud?.showMessage(GameTexts.levelCompleted, "#4ade80", true);
            const used = candlesManager?.playerUsedCandles ?? 0;
            sessionStorage.setItem(
              GAMEOVER_KEY,
              JSON.stringify({
                folder: currentFolder,
                levelId: currentLevelId,
                isWin: true,
                timeMs: Math.floor(elapsedMs),
                candlesUsed: used,
              }),
            );
            setTimeout(navigateToGameOver, 1200);
          } else if (!lockedExitNotified) {
            lockedExitNotified = true;
            hud?.showMessage(GameTexts.closedDoor, "#aaaaaa");
          }
        } else {
          lockedExitNotified = false;
        }
      }

      // Traps
      for (const trap of traps) {
        trap.checkAndTrigger(player);
        trap.update(dt);
      }

      // Player death: when dying starts, show player "in clear" above fading world (Task 2)
      if (player.isDying && !playerDyingDetected && shadowSystem) {
        playerDyingDetected = true;

        // Snapshot current state for death overlay data
        sessionStorage.setItem(
          GAMEOVER_KEY,
          JSON.stringify({
            folder: currentFolder,
            levelId: currentLevelId,
            isWin: false,
            timeMs: Math.floor(elapsedMs),
            candlesUsed: candlesManager?.playerUsedCandles ?? 0,
          }),
        );

        // Show "Game Over" message
        hud?.showMessage(GameTexts.gameOver, "#ff4444", true);

        // Create a sprite of the player in world coords, added to playerDeathContainer
        // (which mirrors camera transform — so world coords map to correct screen position)
        const ds = new Sprite(player.mainSprite.texture);
        ds.anchor.set(0.5);
        ds.x = player.worldCenter.x;
        ds.y = player.worldCenter.y;
        ds.scale.x = player.mainSprite.scale.x;
        shadowSystem.playerDeathContainer.addChild(ds);
        deathOverlaySprite = ds;

        // Fade world sprites to black over 1.5s; playerDeathContainer stays fully visible
        shadowSystem.startDeathFade(1500);

        // Hide the player from the objects pipeline to avoid double-drawing
        player.mainSprite.visible = false;
      }

      // Sync death overlay sprite with the ongoing death animation each frame
      if (deathOverlaySprite && player.isDying) {
        deathOverlaySprite.texture = player.mainSprite.texture;
        deathOverlaySprite.scale.x = player.mainSprite.scale.x;
      }

      // Navigate to gameover when death animation completes
      if (player.isDead && !gameEnded) {
        gameEnded = true;
        setTimeout(navigateToGameOver, 800);
      }

      // Cell text triggers (tutorial / ambient hints)
      // Sviluppo A: testo fisso finché player è sulla cella; B: aggiunge al diario
      const playerCell = player.currentCell;
      if (hud) {
        if (playerCell) {
          const text = grid.cellTriggerTexts.get(playerCell);
          if (text) {
            hud.showCellText(text);
            // Sviluppo B: prima volta su questa cella → aggiungi al diario
            if (!triggeredCells.has(playerCell)) {
              triggeredCells.add(playerCell);
              hud.addToDiary(text, elapsedMs);
            }
          } else {
            hud.hideCellText();
          }
        } else {
          hud.hideCellText();
        }
      }

      // Exit pulsation
      exit?.update(dt);

      // B2: animazione lucchetto sopra la porta di uscita
      if (keyOverlayText) {
        keyOverlayTimer += dt;
        keyOverlayText.y = (exit?.centerY ?? 0) - 80 + Math.sin(keyOverlayTimer * 0.003) * 8;
        keyOverlayText.alpha = 0.85 + Math.sin(keyOverlayTimer * 0.004) * 0.15;
      }

      // Braziers
      for (const b of braziers) b.update(dt);

      // Grid (door animations etc.)
      grid.update(dt);

      // Camera follow
      const z = camera.getZoom();
      const targetX = player.worldCenter.x - GAME_W / 2 / z;
      const targetY = player.worldCenter.y + GAME_H / 2 / z;
      camera.x = Numbers.lerp(camera.x, targetX, 0.1);
      camera.y = Numbers.lerp(camera.y, targetY, 0.1);
      camera.updateShake();

      // Torch fire particles (C# CandleFireParticleSystem)
      if (torchFireParticles) {
        if (!player.isDead && !player.isDying && player.surroundingLight) {
          const p = player.surroundingLight.position;
          torchFireParticles.addParticles(new Point(p.x, p.y));
        }
        torchFireParticles.update(ticker);
      }

      if (shadowSystem) shadowSystem.update(app!, dt);
    });

    // Pause/resume on tab visibility change
    visibilityHandler = () => {
      if (!app) return;
      if (document.hidden) {
        app.ticker.stop();
        SoundManagerInstance.pauseAll();
      } else {
        const isPortrait =
          orientationOverlay?.classList.contains("visible") ?? false;
        if (!isPortrait) app.ticker.start();
        SoundManagerInstance.resumeAll();
      }
    };
    document.addEventListener("visibilitychange", visibilityHandler);
  } catch (err) {
    console.error("[gamebootstrap] Failed to load level:", err);
  }
}

/** Rectangle intersection helper. */
function intersects(a: Rectangle, b: Rectangle): boolean {
  return (
    a.x < b.x + b.width &&
    a.x + a.width > b.x &&
    a.y < b.y + b.height &&
    a.y + a.height > b.y
  );
}

function resize() {
  if (!app || !gameContainer) return;

  const containerW = gameContainer.clientWidth;
  const containerH = gameContainer.clientHeight;

  if (containerW === 0 || containerH === 0) return;

  if (orientationOverlay) {
    const isPortrait = containerH > containerW;
    orientationOverlay.classList.toggle("visible", isPortrait);
    if (isPortrait) {
      app.ticker.stop();
    } else {
      app.ticker.start();
    }
  }

  const scale = Math.min(containerW / GAME_W, containerH / GAME_H);
  const canvasW = Math.floor(GAME_W * scale);
  const canvasH = Math.floor(GAME_H * scale);

  app.renderer.resize(GAME_W, GAME_H);
  app.canvas.style.width = canvasW + "px";
  app.canvas.style.height = canvasH + "px";
  app.stage.scale.set(1);
  app.stage.position.set(0, 0);
}

export function destroyGame() {
  if (!app) return;

  if (visibilityHandler) {
    document.removeEventListener("visibilitychange", visibilityHandler);
    visibilityHandler = null;
  }
  SoundManagerInstance.stopAll();
  hud?.destroy();
  hud = null;
  deathOverlaySprite = null;
  torchFireParticles?.dispose();
  torchFireParticles = null;
  player?.destroy();
  player = null;
  controller?.dispose();
  controller = null;
  for (const enemy of enemies) {
    if (enemy.surroundingLight)
      shadowSystem?.removeLight(enemy.surroundingLight);
    enemy.destroy();
  }
  enemies = [];
  candlesManager?.destroy();
  candlesManager = null;
  candlePacks = [];
  key = null;
  exit = null;
  keyOverlayText?.destroy();
  keyOverlayText = null;
  keyOverlayTimer = 0;
  traps = [];
  for (const b of braziers) {
    shadowSystem?.removeLight(b.surroundingLight);
    if (grid) b.destroy(grid.objectsContainer);
  }
  braziers = [];

  resizeObserver?.disconnect();
  resizeObserver = null;
  orientationOverlay = null;
  camera = null;
  grid = null;
  shadowSystem?.destroy();
  shadowSystem = null;

  try {
    (screen.orientation as unknown as { unlock: () => void }).unlock();
  } catch {
    /* not supported on all browsers */
  }
  app.destroy(true, { children: true });
  app = null;
  gameContainer = null;
}
