import Navigo from 'navigo';
import { renderMenuPage } from './menu';
import { renderLevelSelectPage } from './levelselect';
import { renderFolderLevelsPage } from './folderlevels';
import { renderGameOverPage } from './gameover';
import { initGame, destroyGame } from './gamebootstrap';

const router = new Navigo('/', {
    hash: true,
});

export function initializeRouter() {
    const appElement = document.getElementById('app');

    if (!appElement) {
        console.error('Elemento #app non trovato!');
        return router;
    }

    router
        .on(() => renderMenuPage(appElement))
        .on('/', () => renderMenuPage(appElement!))
        .on('/levels', () => renderLevelSelectPage(appElement!))
        .on('/levels/:folder', (match) => {
            const folder = match?.data?.['folder'] ?? '';
            renderFolderLevelsPage(appElement!, folder);
        })
        .on(
            '/game/:folder/:id',
            (match) => {
                const folder = match?.data?.['folder'] ?? '';
                const levelId = match?.data?.['id'] ?? '1';
                initGame(appElement!, folder, levelId);
            },
            {
                leave: (done) => {
                    destroyGame();
                    done();
                },
            },
        )
        .on('/gameover', () => renderGameOverPage(appElement!))
        .notFound(() => renderMenuPage(appElement!))
        .resolve();

    router.updatePageLinks();

    return router;
}

export default router;
