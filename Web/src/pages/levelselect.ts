import router from './router';
import { LEVEL_FOLDERS } from '../generatedLevelManifest';

export function renderLevelSelectPage(container: HTMLElement) {
    const folderButtons = LEVEL_FOLDERS.map(({ folder, displayName }) =>
        `<a href="/levels/${folder}" class="button primary" data-navigo>${displayName}</a>`
    ).join('\n                ');

    container.innerHTML = `
<main>
    <section class="menu">
        <div class="menu-content">
            <h1 class="title">LIVELLI</h1>
            <div class="level-grid">
                ${folderButtons}
            </div>
            <nav class="menu-actions">
                <a href="/" class="button" data-navigo>MENU</a>
            </nav>
        </div>
    </section>
</main>
`;

    router.updatePageLinks();
}
