import router from './router';
import { unlockHowler, isAudioUnlocked } from '../services/AudioUnlocker';

export function renderMenuPage(container: HTMLElement) {
    container.innerHTML = `
<main>
    <section class="menu">
        <div class="menu-content">
            <h1 class="title">MARK IN DEDARK</h1>
            <nav class="menu-actions">
                <a href="/levels" class="button primary" data-navigo>GIOCA</a>
                <a class="button" id="audio-button">ATTIVA AUDIO</a>
            </nav>
        </div>
    </section>
</main>
`;

    router.updatePageLinks();

    const audioButton = container.querySelector<HTMLAnchorElement>('#audio-button');
    if (audioButton) {
        if (isAudioUnlocked()) {
            audioButton.textContent = 'Audio attivo \u2713';
            audioButton.classList.add('disabled');
        } else {
            audioButton.addEventListener('click', async () => {
                await unlockHowler();
                audioButton.textContent = 'Audio attivo \u2713';
                audioButton.classList.add('disabled');
            });
        }
    }
}
