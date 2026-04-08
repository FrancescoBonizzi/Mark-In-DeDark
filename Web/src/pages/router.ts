import Navigo from "navigo";
import { renderFolderLevelsPage } from "./folderlevels";
import { destroyGame, initGame } from "./gamebootstrap";
import { renderGameOverPage } from "./gameover";
import { renderLevelDesignGuidelinesPage } from "./levelDesignGuidelines";
import { destroyLevelEditor, renderLevelEditorPage } from "./leveleditor";
import { renderLevelSelectPage } from "./levelselect";
import { renderMenuPage } from "./menu";

const router = new Navigo("/", {
  hash: true,
});

export function initializeRouter() {
  const appElement = document.getElementById("app");

  if (!appElement) {
    console.error("Elemento #app non trovato!");
    return router;
  }

  router
    .on(() => renderMenuPage(appElement))
    .on("/", () => renderMenuPage(appElement!))
    .on("/levels", () => renderLevelSelectPage(appElement!))
    .on("/levels/:folder", (match) => {
      const folder = match?.data?.["folder"] ?? "";
      renderFolderLevelsPage(appElement!, folder);
    })
    .on(
      "/game/:folder/:id",
      (match) => {
        const folder = match?.data?.["folder"] ?? "";
        const levelId = match?.data?.["id"] ?? "1";
        initGame(appElement!, folder, levelId);
      },
      {
        leave: (done) => {
          destroyGame();
          done();
        },
      },
    )
    .on("/gameover", () => renderGameOverPage(appElement!))
    .on("/level-editor", () => renderLevelEditorPage(appElement!), {
      leave: (done) => {
        destroyLevelEditor();
        done();
      },
    })
    .on("/level-editor/guidelines", () =>
      renderLevelDesignGuidelinesPage(appElement!),
    )
    .notFound(() => renderMenuPage(appElement!))
    .resolve();

  router.updatePageLinks();

  return router;
}

export default router;
