import router from './router';

const TOTAL_LEVELS = 8;

export function renderLevelSelectPage(container: HTMLElement) {
    const levelButtons = Array.from({ length: TOTAL_LEVELS }, (_, i) => {
        const id = i + 1;
        return `<a href="/game/${id}" class="button primary" data-navigo>Livello ${id}</a>`;
    }).join('\n                ');

    container.innerHTML = `
<main>
    <section class="menu">
        <div class="menu-content">
            <h1 class="title">LIVELLI</h1>
            <div class="level-grid">
                ${levelButtons}
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
