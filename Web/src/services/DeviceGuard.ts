/**
 * DeviceGuard — rilevamento dispositivi incompatibili.
 * Il gioco richiede una tastiera fisica. I dispositivi touch-only
 * (smartphone, tablet senza tastiera) vengono bloccati.
 */

export function hasPhysicalKeyboard(): boolean {
    return window.matchMedia('(hover: hover) and (pointer: fine)').matches;
}

/**
 * Se il dispositivo non ha tastiera, svuota il container e mostra
 * la schermata di blocco. Ritorna true se il gioco deve fermarsi.
 */
export function blockIfNoKeyboard(container: HTMLElement): boolean {
    if (hasPhysicalKeyboard()) return false;

    container.innerHTML = `
<div class="device-block-overlay">
    <div class="device-block-content">
        <div class="device-block-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="80" height="80"
                 fill="none" stroke="currentColor" stroke-width="1.2"
                 stroke-linecap="round" stroke-linejoin="round">
                <rect x="2" y="4" width="20" height="14" rx="2" />
                <circle cx="6"  cy="8"  r="0.5" fill="currentColor"/>
                <circle cx="10" cy="8"  r="0.5" fill="currentColor"/>
                <circle cx="14" cy="8"  r="0.5" fill="currentColor"/>
                <circle cx="18" cy="8"  r="0.5" fill="currentColor"/>
                <circle cx="6"  cy="12" r="0.5" fill="currentColor"/>
                <circle cx="18" cy="12" r="0.5" fill="currentColor"/>
                <rect x="8" y="14" width="8" height="2" rx="1" fill="currentColor"/>
                <line x1="4" y1="2" x2="20" y2="22" stroke="#cc2222" stroke-width="1.5"/>
            </svg>
        </div>
        <h1 class="device-block-title">TASTIERA RICHIESTA</h1>
        <p class="device-block-text">
            Mark In DeDark è giocabile solo con una tastiera fisica.<br>
            I dispositivi touch e gli schermi piccoli non sono supportati.
        </p>
        <p class="device-block-hint">Apri il gioco da un computer desktop.</p>
    </div>
</div>
`;
    return true;
}
