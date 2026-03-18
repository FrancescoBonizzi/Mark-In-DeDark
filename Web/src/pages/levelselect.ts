import router from './router';

export function renderLevelSelectPage(container: HTMLElement) {
    container.innerHTML = `
<main>
    <section class="menu">
        <div class="menu-content">
            <h1 class="title">LIVELLI</h1>
            <nav class="menu-actions">
                <a href="/game/1" class="button primary" data-navigo>Livello 1</a>
                <a href="/" class="button" data-navigo>MENU</a>
            </nav>
        </div>
    </section>
</main>
`;

    router.updatePageLinks();
}
