import router from './router';
import { LEVEL_FOLDERS } from '../generatedLevelManifest';

const GAMEOVER_KEY = 'mark-in-dedark-gameover';

interface GameOverPayload {
    folder: string;
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
    const folder = payload?.folder ?? '';
    const timeMs = payload?.timeMs ?? 0;
    const candlesUsed = payload?.candlesUsed ?? 0;

    const title = isWin ? 'COMPLETATO!' : 'GAME OVER';
    const nextLevelId = levelId + 1;
    const folderManifest = LEVEL_FOLDERS.find(f => f.folder === folder);
    const nextLevelEntry = folderManifest?.levels.find(l => l.id === nextLevelId);
    const hasNextLevel = isWin && nextLevelEntry !== undefined;

    const primaryBtn = hasNextLevel
        ? `<a href="/game/${folder}/${nextLevelId}" class="button primary" data-navigo>${nextLevelEntry!.name || String(nextLevelId)}</a>`
        : `<a href="/game/${folder}/${levelId}" class="button primary" data-navigo>RIPROVA</a>`;

    const retryBtn = hasNextLevel
        ? `<a href="/game/${folder}/${levelId}" class="button" data-navigo>RIPROVA</a>`
        : '';

    container.innerHTML = `
<main>
    <div class="page-embers" aria-hidden="true">
        <span class="ember"></span><span class="ember"></span><span class="ember"></span>
        <span class="ember"></span><span class="ember"></span><span class="ember"></span>
        <span class="ember"></span><span class="ember"></span>
    </div>
    <section class="gameover">
        <div class="menu-content">
            <h1 class="title${isWin ? ' title-win' : ''}">${title}</h1>
            <div class="gameover-stats">
                <div>${folder} — ${levelId}</div>
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
