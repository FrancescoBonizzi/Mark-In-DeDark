/** In-game HUD overlay (candles left + elapsed time). */
export class GameHud {
    private readonly _el: HTMLElement;
    private readonly _timerEl: HTMLElement;
    private readonly _candlesEl: HTMLElement;

    constructor(parent: HTMLElement) {
        this._el = document.createElement('div');
        this._el.id = 'game-hud';

        this._timerEl = document.createElement('span');
        this._timerEl.textContent = '00:00';

        this._candlesEl = document.createElement('span');
        this._candlesEl.textContent = 'candele 0';

        this._el.appendChild(this._timerEl);
        this._el.appendChild(this._candlesEl);
        parent.appendChild(this._el);
    }

    update(candlesLeft: number, elapsedMs: number): void {
        const sec = Math.floor(elapsedMs / 1000);
        const m = Math.floor(sec / 60).toString().padStart(2, '0');
        const s = (sec % 60).toString().padStart(2, '0');
        this._timerEl.textContent = `${m}:${s}`;
        this._candlesEl.textContent = `candele ${candlesLeft}`;
    }

    destroy(): void {
        this._el.remove();
    }
}
