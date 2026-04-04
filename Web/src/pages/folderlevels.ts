import router from './router';
import { LEVEL_FOLDERS } from '../generatedLevelManifest';

export function renderFolderLevelsPage(container: HTMLElement, folder: string) {
    const manifest = LEVEL_FOLDERS.find(f => f.folder === folder);

    if (!manifest) {
        window.location.hash = '#/levels';
        return;
    }

    const levelButtons = manifest.levels.map(levelNum =>
        `<a href="/game/${folder}/${levelNum}" class="button primary" data-navigo>${levelNum}</a>`
    ).join('\n                ');

    container.innerHTML = `
<main>
    <section class="menu">
        <div class="menu-content">
            <h1 class="title">${manifest.displayName}</h1>
            <div class="level-grid">
                ${levelButtons}
            </div>
            <nav class="menu-actions">
                <a href="/levels" class="button" data-navigo>INDIETRO</a>
            </nav>
        </div>
    </section>
</main>
`;

    router.updatePageLinks();
}
