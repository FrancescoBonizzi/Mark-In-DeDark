import { Application } from 'pixi.js';

const GAME_W = 1280;
const GAME_H = 720;

let app: Application | null = null;
let gameContainer: HTMLDivElement | null = null;
let orientationOverlay: HTMLDivElement | null = null;
let resizeObserver: ResizeObserver | null = null;

export async function initGame(container: HTMLElement, _levelId: string) {
    if (app) {
        destroyGame();
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
        await (screen.orientation as any).lock('landscape');
    } catch { /* not supported on all browsers */ }

    resizeObserver = new ResizeObserver(resize);
    resizeObserver.observe(gameContainer);
    resize();

    gameContainer.appendChild(app.canvas);
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

    resizeObserver?.disconnect();
    resizeObserver = null;
    orientationOverlay = null;
    try {
        (screen.orientation as any).unlock();
    } catch { /* not supported on all browsers */ }
    app.destroy(true, { children: true });
    app = null;
    gameContainer = null;
}
