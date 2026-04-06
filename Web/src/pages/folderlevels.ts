import router from './router';
import { LEVEL_FOLDERS } from '../generatedLevelManifest';
import { blockIfNoKeyboard } from '../services/DeviceGuard';

export function renderFolderLevelsPage(container: HTMLElement, folder: string) {
    if (blockIfNoKeyboard(container)) return;

    const manifest = LEVEL_FOLDERS.find(f => f.folder === folder);

    if (!manifest) {
        window.location.hash = '#/levels';
        return;
    }

    const isDefault = folder === 'default';
    const pageTitle = isDefault ? 'La Discesa di Mark' : manifest.displayName;
    const pageSubtitle = isDefault ? 'STORIA UFFICIALE' : 'ESPANSIONE';

    const levelButtons = manifest.levels.map((entry) => `
<a href="/game/${folder}/${entry.id}" class="level-entry" data-navigo>
    <span class="level-entry__num">${String(entry.id).padStart(2, '0')}</span>
    <span class="level-entry__divider">—</span>
    <span class="level-entry__name">${entry.name || `Livello ${entry.id}`}</span>
    <span class="level-entry__arrow">▶</span>
</a>`).join('');

    container.innerHTML = `
<div class="folder-levels-page">
    <div class="folder-levels-page__bg-glow"></div>

    <header class="folder-levels-page__header">
        <a href="/levels" class="back-link" data-navigo>← INDIETRO</a>
        <div>
            <div class="folder-levels-page__eyebrow">${pageSubtitle}</div>
            <h1 class="folder-levels-page__title">${pageTitle}</h1>
        </div>
    </header>

    <main class="folder-levels-page__main">
        <div class="level-list">
            ${levelButtons}
        </div>
    </main>
</div>
`;

    router.updatePageLinks();
}
