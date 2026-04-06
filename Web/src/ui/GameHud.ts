const POPUP_LIFETIME_MS = 6000;
const POPUP_FLOAT_PX   = 60;   // total upward travel over lifetime
const POPUP_START_Y    = 78;   // px from top of viewport
const POPUP_STACK_GAP  = 50;   // px between stacked popups

interface PopupEntry {
    el:        HTMLElement;
    startY:    number;
    elapsed:   number;
    text:      string;
}

/** In-game HUD overlay (candles left + contextual popups). */
export class GameHud {
    private readonly _el:        HTMLElement;
    private readonly _candlesEl: HTMLElement;
    private readonly _popups:    PopupEntry[] = [];

    constructor(parent: HTMLElement) {
        this._el = document.createElement('div');
        this._el.id = 'game-hud';

        this._candlesEl = document.createElement('span');
        this._candlesEl.textContent = 'candele 0';

        this._el.appendChild(this._candlesEl);
        parent.appendChild(this._el);
    }

    update(candlesLeft: number, _elapsedMs: number): void {
        this._candlesEl.textContent = `candele ${candlesLeft}`;
    }

    showMessage(text: string, color: string, isImportant = false): void {
        // Deduplication: skip if last active popup has the same text
        const last = this._popups[this._popups.length - 1];
        if (last && last.text === text) return;

        // Stack below the lowest current popup
        let startY = POPUP_START_Y;
        for (const p of this._popups) {
            const bottom = p.startY + (isImportant ? 38 : 26);
            if (bottom + POPUP_STACK_GAP > startY) startY = bottom + POPUP_STACK_GAP;
        }

        const el = document.createElement('div');
        el.className = 'hud-popup' + (isImportant ? ' important' : '');
        el.style.color = color;
        el.style.top   = `${startY}px`;
        el.style.opacity = '0';
        el.textContent = text;
        this._el.appendChild(el);

        this._popups.push({ el, startY, elapsed: 0, text });
    }

    updatePopups(deltaMs: number): void {
        for (let i = this._popups.length - 1; i >= 0; i--) {
            const p = this._popups[i]!;
            p.elapsed += deltaMs;

            const t = Math.min(p.elapsed / POPUP_LIFETIME_MS, 1);
            const alpha = 4 * t * (1 - t);    // parabolic fade-in/out
            const dy    = t * POPUP_FLOAT_PX;  // float upward

            p.el.style.opacity = String(alpha);
            p.el.style.top = `${p.startY - dy}px`;

            if (p.elapsed >= POPUP_LIFETIME_MS) {
                p.el.remove();
                this._popups.splice(i, 1);
            }
        }
    }

    destroy(): void {
        this._el.remove();
    }
}
