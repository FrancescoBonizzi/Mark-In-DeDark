import { LEVEL_FOLDERS } from "../generatedLevelManifest";
import { blockIfNoKeyboard } from "../services/DeviceGuard";
import router from "./router";

export function renderLevelSelectPage(container: HTMLElement) {
  if (blockIfNoKeyboard(container)) return;

  const defaultFolder = LEVEL_FOLDERS.find((f) => f.folder === "default");
  const expansionFolders = LEVEL_FOLDERS.filter((f) => f.folder !== "default");

  const defaultCard = defaultFolder
    ? `
<a href="/levels/default" class="story-card" data-navigo>
    <div class="story-card__eyes" aria-hidden="true"></div>
    <div class="story-card__content">
        <div class="story-card__eyebrow">STORIA UFFICIALE</div>
        <h2 class="story-card__title">Il ventre del padre</h2>
        <p class="story-card__desc">
            ${defaultFolder.levels.length} livell${defaultFolder.levels.length === 1 ? "o" : "i"} &nbsp;·&nbsp; Atto I–IV
        </p>
        <span class="story-card__cta">INIZIA ▶</span>
    </div>
</a>`
    : "";

  const expansionCards =
    expansionFolders.length > 0
      ? `
<section class="expansions">
    <h3 class="expansions__label">— ESPANSIONI —</h3>
    <div class="expansions__grid">
        ${expansionFolders
          .map(
            (f) => `
        <a href="/levels/${f.folder}" class="expansion-card" data-navigo>
            <span class="expansion-card__border"></span>
            <span class="expansion-card__name">${f.displayName}</span>
            <span class="expansion-card__count">${f.levels.length} livell${f.levels.length === 1 ? "o" : "i"}</span>
        </a>`,
          )
          .join("")}
    </div>
</section>`
      : "";

  container.innerHTML = `
<div class="level-select-page">
    <div class="level-select-page__bg-glow"></div>
    <div class="page-embers" aria-hidden="true">
        <span class="ember"></span><span class="ember"></span><span class="ember"></span>
        <span class="ember"></span><span class="ember"></span><span class="ember"></span>
        <span class="ember"></span><span class="ember"></span>
    </div>

    <header class="level-select-page__header">
        <a href="/" class="back-link" data-navigo>← MENU</a>
        <h1 class="level-select-page__title">LIVELLI</h1>
    </header>

    <main class="level-select-page__main">
        ${defaultCard}
        ${expansionCards}
    </main>
</div>
`;

  router.updatePageLinks();
}
