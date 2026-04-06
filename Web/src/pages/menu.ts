import { AssetsRoot } from "../assets/constants";
import { isAudioUnlocked, unlockHowler } from "../services/AudioUnlocker";
import { blockIfNoKeyboard } from "../services/DeviceGuard";
import router from "./router";

export function renderMenuPage(container: HTMLElement) {
  if (blockIfNoKeyboard(container)) return;

  const logoSrc = `${AssetsRoot}assets/logo.png`;

  container.innerHTML = `
<div class="main-menu">
    <div class="main-menu__bg-glow"></div>
    <div class="main-menu__embers" aria-hidden="true">
        <span class="ember"></span><span class="ember"></span><span class="ember"></span>
        <span class="ember"></span><span class="ember"></span><span class="ember"></span>
        <span class="ember"></span><span class="ember"></span>
    </div>

    <div class="main-menu__content">
        <div class="main-menu__logo-wrap">
            <div class="main-menu__logo-glow"></div>
            <img class="main-menu__logo" src="${logoSrc}" alt="Mark In DeDark" />
        </div>

        <nav class="main-menu__nav">
            <a href="/levels" class="menu-btn menu-btn--primary" data-navigo>
                <span class="menu-btn__border menu-btn__border--tl"></span>
                <span class="menu-btn__border menu-btn__border--br"></span>
                ▶&nbsp; GIOCA
            </a>
            <a class="menu-btn" id="audio-button">
                <span class="menu-btn__border menu-btn__border--tl"></span>
                <span class="menu-btn__border menu-btn__border--br"></span>
                ♪&nbsp; ATTIVA AUDIO
            </a>
            <a href="/level-editor" class="menu-btn menu-btn--secondary" data-navigo>
                <span class="menu-btn__border menu-btn__border--tl"></span>
                <span class="menu-btn__border menu-btn__border--br"></span>
                ✦&nbsp; LEVEL EDITOR
            </a>
        </nav>
    </div>
</div>
`;

  router.updatePageLinks();

  const audioButton =
    container.querySelector<HTMLAnchorElement>("#audio-button");
  if (audioButton) {
    if (isAudioUnlocked()) {
      audioButton.innerHTML = audioButton.innerHTML.replace(
        "ATTIVA AUDIO",
        "AUDIO ATTIVO ✓",
      );
      audioButton.classList.add("menu-btn--disabled");
    } else {
      audioButton.addEventListener("click", async () => {
        await unlockHowler();
        audioButton.innerHTML = audioButton.innerHTML.replace(
          "ATTIVA AUDIO",
          "AUDIO ATTIVO ✓",
        );
        audioButton.classList.add("menu-btn--disabled");
      });
    }
  }
}
