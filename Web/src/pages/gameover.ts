import router from './router';

export function renderGameOverPage(container: HTMLElement) {
    container.innerHTML = `
<main>
    <section class="gameover">
        <div class="menu-content">
            <h1 class="title">GAME OVER</h1>
            <nav class="menu-actions">
                <a href="/" class="button primary" data-navigo>MENU</a>
            </nav>
        </div>
    </section>
</main>
`;

    router.updatePageLinks();
}
