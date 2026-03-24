import { Application, Rectangle, TextureSource } from 'pixi.js';
import { loadAssets } from '../assets/AssetsLoader';
import { loadAndParseLevel } from '../grid/GridLoader';
import { createLevelConfig } from '../grid/LevelConfig';
import { createLevelCustomization } from '../grid/LevelCustomization';
import { GameGrid, GRID_MAX_HEIGHT } from '../grid/GameGrid';
import { CELL_SIZE } from '../grid/GameCell';
import { GroundTypes } from '../grid/enums';
import Camera from '../world/Camera';
import { ShadowSystem } from '../lighting/ShadowSystem';
import Controller from '../interaction/Controller';
import { Player } from '../characters/Player';
import { CandlesManager } from '../objects/CandlesManager';
import { CandlePack } from '../objects/CandlePack';
import { Key } from '../objects/Key';
import { Exit } from '../objects/Exit';
import { InstantlyDieTrap } from '../objects/InstantlyDieTrap';
import { TimedBreakableTrap } from '../objects/TimedBreakableTrap';
import { GoblinTriggerTrap } from '../objects/GoblinTriggerTrap';
import { Enemy } from '../characters/Enemy';
import { createEnemiesFromGrid } from '../characters/enemies/EnemyFactory';
import { Goblin } from '../characters/enemies/Goblin';
import Numbers from '../services/Numbers';

const GAME_W = 1280;
const GAME_H = 720;

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
let enemies: Enemy[] = [];

const hasPhysicalKeyboard = (): boolean =>
    window.matchMedia('(hover: hover) and (pointer: fine)').matches;

export async function initGame(container: HTMLElement, levelIdParam: string) {
    if (app) {
        destroyGame();
    }

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

    gameContainer = container.querySelector<HTMLDivElement>('#game-container');
    if (!gameContainer) {
        console.error('Game container non trovato');
        return;
    }

    orientationOverlay = container.querySelector<HTMLDivElement>('#orientation-overlay');

    app = new Application();

    TextureSource.defaultOptions.scaleMode = 'nearest';

    await app.init({
        background: '#0a0a0a',
        width: GAME_W,
        height: GAME_H,
        premultipliedAlpha: false,
        antialias: true,
        autoDensity: true,
        resolution: Math.min(window.devicePixelRatio || 1, 2),
    });

    try {
        await (screen.orientation as unknown as { lock: (o: string) => Promise<void> }).lock('landscape');
    } catch { /* not supported on all browsers */ }

    resizeObserver = new ResizeObserver(resize);
    resizeObserver.observe(gameContainer);
    resize();

    gameContainer.appendChild(app.canvas);

    const levelId = parseInt(levelIdParam, 10) || 1;

    try {
        const [assets, bundle] = await Promise.all([
            loadAssets(),
            loadAndParseLevel(levelId),
        ]);

        const levelConfig = createLevelConfig(bundle.envTexturePackId, assets.spriteSheets);
        const levelCustomization = createLevelCustomization(levelId);

        camera = new Camera(app, GRID_MAX_HEIGHT);
        grid = new GameGrid(bundle, levelConfig, levelCustomization);

        // Close all interactive doors after grid initialization
        grid.closeAllDoors();

        shadowSystem = new ShadowSystem(
            app, camera,
            grid.groundContainer,
            grid.groundNormalContainer,
            grid.objectsContainer,
            grid.objectsNormalContainer,
        );
        shadowSystem.ambientColor = levelCustomization.penumbraAmbientColor;
        shadowSystem.ambientAlpha = levelCustomization.penumbraAmbientAlpha;
        shadowSystem.normalMapAmbienceColor = levelCustomization.normalMapAmbienceColor;
        shadowSystem.addHullsFromWalls(grid.allWalls);
        shadowSystem.addHullsFromPillars(grid.allPillars);

        // Create player and controller
        controller = new Controller();
        player = new Player(
            grid,
            assets.spriteSheets,
            grid.objectsContainer,
            grid.objectsNormalContainer,
        );

        if (player.surroundingLight) {
            shadowSystem.addLight(player.surroundingLight);
        }

        // CandlesManager (player starts with 3 candles)
        candlesManager = new CandlesManager(assets.spriteSheets, grid.objectsContainer, shadowSystem, 3);
        player.setCandlesManager(candlesManager);

        // CandlePacks from grid placements
        candlePacks = grid.candlePlacements.map(p => new CandlePack(
            assets.spriteSheets,
            p.col * CELL_SIZE + CELL_SIZE / 2,
            p.row * CELL_SIZE + CELL_SIZE / 2,
            p.count,
            grid!.objectsContainer,
        ));

        // Key
        key = new Key(
            assets.spriteSheets,
            grid.keyCol * CELL_SIZE + CELL_SIZE / 2,
            grid.keyRow * CELL_SIZE + CELL_SIZE / 2,
            grid.objectsContainer,
        );

        // Exit
        exit = new Exit(
            grid.exitCol * CELL_SIZE + CELL_SIZE / 2,
            grid.exitRow * CELL_SIZE + CELL_SIZE / 2,
            grid.exitDoor,
            grid,
        );

        // Traps
        traps = grid.trapPlacements.map(t => {
            const cx = t.col * CELL_SIZE;
            const cy = t.row * CELL_SIZE;
            if (t.groundType === GroundTypes.TrappolaMorteIstantanea) {
                return new InstantlyDieTrap(assets.spriteSheets, cx, cy, CELL_SIZE, grid!.groundContainer);
            } else if (t.groundType === GroundTypes.TrappolaTemporizzata) {
                return new TimedBreakableTrap(assets.spriteSheets, cx, cy, CELL_SIZE, grid!.groundContainer);
            } else {
                // TriggerGoblin — spawns a Goblin on trigger
                const triggerCol = t.col;
                const triggerRow = t.row;
                return new GoblinTriggerTrap(assets.spriteSheets, cx, cy, CELL_SIZE, grid!.groundContainer, () => {
                    if (!grid || !shadowSystem || !player || !candlesManager) return;
                    const cell = grid.getCell(triggerCol, triggerRow);
                    if (!cell) return;
                    const goblin = new Goblin(
                        grid, assets.spriteSheets,
                        grid.objectsContainer, grid.objectsNormalContainer,
                        cell.centerX, cell.centerY,
                        player, candlesManager,
                    );
                    if (goblin.surroundingLight) shadowSystem.addLight(goblin.surroundingLight);
                    enemies.push(goblin);
                    console.log('[GoblinTrigger] goblin spawned');
                });
            }
        });

        // Create enemies from grid placements
        enemies = createEnemiesFromGrid(
            grid, assets.spriteSheets,
            grid.objectsContainer, grid.objectsNormalContainer,
            player, candlesManager, shadowSystem,
        );

        // Center camera on player start
        const zoom = camera.getZoom();
        const startX = grid.playerStartCol * CELL_SIZE + CELL_SIZE / 2;
        const startY = grid.playerStartRow * CELL_SIZE + CELL_SIZE / 2;
        camera.x = startX - GAME_W / 2 / zoom;
        camera.y = startY + GAME_H / 2 / zoom;

        app.ticker.add((ticker) => {
            if (!camera || !grid || !player || !controller) return;
            const dt = ticker.deltaMS;

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
                    exit?.setOpen();
                    console.log('[Key] raccolta — uscita aperta');
                }
            } else {
                key?.update(dt);
            }

            for (let i = candlePacks.length - 1; i >= 0; i--) {
                const pack = candlePacks[i]!;
                pack.update(dt);
                if (!pack.isCollected && intersects(player.collisionRectangle, pack.collisionRectangle)) {
                    pack.collect();
                    candlesManager?.addCandles(pack.count);
                    console.log(`[CandlePack] raccolto (${pack.count} candele)`);
                }
                if (pack.isGone) candlePacks.splice(i, 1);
            }

            // Exit collision
            if (exit?.isOpen && !player.isDead) {
                if (intersects(player.collisionRectangle, exit.collisionRectangle)) {
                    console.log('[Exit] livello completato!');
                    // TODO Task 9: navigate to gameover/win screen
                }
            }

            // Traps
            for (const trap of traps) {
                trap.checkAndTrigger(player);
                trap.update(dt);
            }

            // Grid (door animations etc.)
            grid.update(dt);

            // Camera follow
            const z = camera.getZoom();
            const targetX = player.worldCenter.x - GAME_W / 2 / z;
            const targetY = player.worldCenter.y + GAME_H / 2 / z;
            camera.x = Numbers.lerp(camera.x, targetX, 0.1);
            camera.y = Numbers.lerp(camera.y, targetY, 0.1);

            if (shadowSystem) shadowSystem.update(app!, dt);
        });

    } catch (err) {
        console.error('[Task7] Failed to load level:', err);
    }
}

/** Rectangle intersection helper. */
function intersects(a: Rectangle, b: Rectangle): boolean {
    return a.x < b.x + b.width &&
        a.x + a.width > b.x &&
        a.y < b.y + b.height &&
        a.y + a.height > b.y;
}

function resize() {
    if (!app || !gameContainer) return;

    const containerW = gameContainer.clientWidth;
    const containerH = gameContainer.clientHeight;

    if (containerW === 0 || containerH === 0) return;

    if (orientationOverlay) {
        const isPortrait = containerH > containerW;
        orientationOverlay.classList.toggle('visible', isPortrait);
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
    app.canvas.style.width = canvasW + 'px';
    app.canvas.style.height = canvasH + 'px';
    app.stage.scale.set(1);
    app.stage.position.set(0, 0);
}

export function destroyGame() {
    if (!app) return;

    player?.destroy();
    player = null;
    controller?.dispose();
    controller = null;
    for (const enemy of enemies) {
        if (enemy.surroundingLight) shadowSystem?.removeLight(enemy.surroundingLight);
        enemy.destroy();
    }
    enemies = [];
    candlesManager?.destroy();
    candlesManager = null;
    candlePacks = [];
    key = null;
    exit = null;
    traps = [];

    resizeObserver?.disconnect();
    resizeObserver = null;
    orientationOverlay = null;
    camera = null;
    grid = null;
    shadowSystem?.destroy();
    shadowSystem = null;

    try {
        (screen.orientation as unknown as { unlock: () => void }).unlock();
    } catch { /* not supported on all browsers */ }
    app.destroy(true, { children: true });
    app = null;
    gameContainer = null;
}
