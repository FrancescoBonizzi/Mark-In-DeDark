import { Application, TextureSource } from 'pixi.js';
import { loadAssets } from '../assets/AssetsLoader';
import { loadAndParseLevel } from '../grid/GridLoader';
import { createLevelConfig } from '../grid/LevelConfig';
import { createLevelCustomization } from '../grid/LevelCustomization';
import { GameGrid, GRID_MAX_HEIGHT } from '../grid/GameGrid';
import { CELL_SIZE } from '../grid/GameCell';
import Camera from '../world/Camera';
import { ShadowSystem } from '../lighting/ShadowSystem';
import { MarkLight } from '../lighting/MarkLight';

const GAME_W = 1280;
const GAME_H = 720;

let app: Application | null = null;
let gameContainer: HTMLDivElement | null = null;
let orientationOverlay: HTMLDivElement | null = null;
let resizeObserver: ResizeObserver | null = null;
let camera: Camera | null = null;
let grid: GameGrid | null = null;
let shadowSystem: ShadowSystem | null = null;
let keydownHandler: ((e: KeyboardEvent) => void) | null = null;
let keyupHandler: ((e: KeyboardEvent) => void) | null = null;

const CAMERA_SPEED = 300; // pixels per second
const pressedKeys = new Set<string>();

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

        // Center camera on player start
        const playerStartX = grid.playerStartCol * CELL_SIZE + CELL_SIZE / 2;
        const playerStartY = grid.playerStartRow * CELL_SIZE + CELL_SIZE / 2;
        const zoom = camera.getZoom();
        camera.x = playerStartX - GAME_W / 2 / zoom;
        camera.y = playerStartY + GAME_H / 2 / zoom;

        // ShadowSystem owns all containers and renders them offline.
        // camera._world stays on stage but has no children (renders nothing).
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

        // Test light at player start position
        const testLight = new MarkLight({
            x: playerStartX,
            y: playerStartY,
            radius: 1200,
            castsShadows: true,
            color1: 0xffaa44,
            color2: 0xff6600,
            distanceSquared: 20_600,
        });
        shadowSystem.addLight(testLight);

        // Arrow key camera navigation for verification
        keydownHandler = (e: KeyboardEvent) => {
            pressedKeys.add(e.key);
        };
        keyupHandler = (e: KeyboardEvent) => {
            pressedKeys.delete(e.key);
        };
        window.addEventListener('keydown', keydownHandler);
        window.addEventListener('keyup', keyupHandler);

        app.ticker.add((ticker) => {
            if (!camera) return;
            const dt = ticker.deltaMS / 1000;
            const step = CAMERA_SPEED * dt;

            if (pressedKeys.has('ArrowLeft')) camera.x -= step;
            if (pressedKeys.has('ArrowRight')) camera.x += step;
            if (pressedKeys.has('ArrowUp')) camera.y -= step;
            if (pressedKeys.has('ArrowDown')) camera.y += step;

            // TODO Task 6: rimuovere — luce segue il centro della camera per il debug
            testLight.position.x = camera.x + GAME_W / 2 / camera.getZoom();
            testLight.position.y = camera.y - GAME_H / 2 / camera.getZoom();

            if (grid) grid.update(ticker.deltaMS);
            if (shadowSystem) shadowSystem.update(app!, ticker.deltaMS);
        });

    } catch (err) {
        console.error('[Task3] Failed to load level:', err);
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

    if (keydownHandler) {
        window.removeEventListener('keydown', keydownHandler);
        keydownHandler = null;
    }
    if (keyupHandler) {
        window.removeEventListener('keyup', keyupHandler);
        keyupHandler = null;
    }
    pressedKeys.clear();

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
