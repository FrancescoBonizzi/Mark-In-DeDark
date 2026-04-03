import router from './router';
import { LEVELS, getLevelName } from '../levels';

const GAMEOVER_KEY = 'mark-in-dedark-gameover';

interface GameOverPayload {
    levelId: number;
    isWin: boolean;
    timeMs: number;
    candlesUsed: number;
}

function formatTime(ms: number): string {
    const sec = Math.floor(ms / 1000);
    const m = Math.floor(sec / 60).toString().padStart(2, '0');
    const s = (sec % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
}

export function renderGameOverPage(container: HTMLElement) {
    let payload: GameOverPayload | null = null;
    try {
        const raw = sessionStorage.getItem(GAMEOVER_KEY);
        if (raw) payload = JSON.parse(raw) as GameOverPayload;
    } catch { /* ignore */ }

    const isWin = payload?.isWin ?? false;
    const levelId = payload?.levelId ?? 1;
    const timeMs = payload?.timeMs ?? 0;
    const candlesUsed = payload?.candlesUsed ?? 0;

    const title = isWin ? 'COMPLETATO!' : 'GAME OVER';
    const nextLevelId = levelId + 1;
    const hasNextLevel = isWin && nextLevelId <= LEVELS.length;

    const primaryBtn = hasNextLevel
        ? `<a href="/game/${nextLevelId}" class="button primary" data-navigo>${nextLevelId} — ${getLevelName(nextLevelId)}</a>`
        : `<a href="/game/${levelId}" class="button primary" data-navigo>RIPROVA</a>`;

    const retryBtn = hasNextLevel
        ? `<a href="/game/${levelId}" class="button" data-navigo>RIPROVA</a>`
        : '';

    container.innerHTML = `
<main>
    <section class="gameover">
        <div class="menu-content">
            <h1 class="title${isWin ? ' title-win' : ''}">${title}</h1>
            <div class="gameover-stats">
                <div>${levelId} — ${getLevelName(levelId)}</div>
                <div>TEMPO ${formatTime(timeMs)}</div>
                <div>CANDELE USATE ${candlesUsed}</div>
            </div>
            <nav class="menu-actions">
                ${primaryBtn}
                ${retryBtn}
                <a href="/" class="button" data-navigo>MENU</a>
            </nav>
        </div>
    </section>
</main>
`;

    router.updatePageLinks();
}
