import { Application, TextureSource } from 'pixi.js';
import { loadAssets } from '../assets/AssetsLoader';
import { loadAndParseLevel } from '../grid/GridLoader';
import { createLevelConfig } from '../grid/LevelConfig';
import { createLevelCustomization } from '../grid/LevelCustomization';
import { GameGrid, GRID_MAX_HEIGHT } from '../grid/GameGrid';
import { CELL_SIZE } from '../grid/GameCell';
import Camera from '../world/Camera';
import { ShadowSystem } from '../lighting/ShadowSystem';
import Controller from '../interaction/Controller';
import { Player } from '../characters/Player';
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

    // Load assets and level
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

        // ShadowSystem owns all containers and renders them offline.
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

        // Center camera instantly on player start position
        const zoom = camera.getZoom();
        const startX = grid.playerStartCol * CELL_SIZE + CELL_SIZE / 2;
        const startY = grid.playerStartRow * CELL_SIZE + CELL_SIZE / 2;
        camera.x = startX - GAME_W / 2 / zoom;
        camera.y = startY + GAME_H / 2 / zoom;

        app.ticker.add((ticker) => {
            if (!camera || !grid || !player || !controller) return;
            const dt = ticker.deltaMS;

            player.handleInput(controller.currentAction);
            player.update(dt);
            grid.update(dt);

            // Smooth camera follow (lerp factor 0.1)
            const z = camera.getZoom();
            const targetX = player.worldCenter.x - GAME_W / 2 / z;
            const targetY = player.worldCenter.y + GAME_H / 2 / z;
            camera.x = Numbers.lerp(camera.x, targetX, 0.1);
            camera.y = Numbers.lerp(camera.y, targetY, 0.1);

            if (shadowSystem) shadowSystem.update(app!, dt);
        });

    } catch (err) {
        console.error('[Task6] Failed to load level:', err);
    }
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
    if (!app) {
        return;
    }

    player?.destroy();
    player = null;
    controller?.dispose();
    controller = null;

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
