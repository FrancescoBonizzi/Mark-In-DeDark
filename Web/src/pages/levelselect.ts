import router from './router';
import { LEVELS } from '../levels';

export function renderLevelSelectPage(container: HTMLElement) {
    const levelButtons = LEVELS.map(({ id, name }) =>
        `<a href="/game/${id}" class="button primary" data-navigo>${id} — ${name}</a>`
    ).join('\n                ');

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
