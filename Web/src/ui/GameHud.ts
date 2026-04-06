const POPUP_LIFETIME_MS = 5000;
const POPUP_FLOAT_PX   = 80;   // total upward travel over lifetime
const HUD_BAR_H        = 44;   // px — altezza barra in basso
const POPUP_START_Y_FROM_BOTTOM = HUD_BAR_H + 14; // px sopra la barra

interface PopupEntry {
    el:        HTMLElement;
    startBottom: number;
    elapsed:   number;
    text:      string;
}

interface DiaryEntry {
    text: string;
    timestamp: number;  // ms elapsed
    read: boolean;
    isEvent: boolean;   // true = messaggio automatico di gioco, false = testo narrativo
}

/** In-game HUD overlay — barra retrò in basso + popup + diario narrativo. */
export class GameHud {
    private readonly _el:        HTMLElement;
    private readonly _candlesEl: HTMLElement;
    private readonly _keyEl:     HTMLElement;
    private readonly _levelEl:   HTMLElement;
    private readonly _popups:    PopupEntry[] = [];

    // Sviluppo A: testo cella attivo (fisso finché player è sulla cella)
    private _cellTextEl: HTMLElement | null = null;
    private _cellTextActive = false;

    // Sviluppo B: diario narrativo
    private readonly _diary:     DiaryEntry[] = [];
    private _diaryOverlay:       HTMLElement | null = null;
    private _diaryBadge:         HTMLElement | null = null;
    private _diaryOpen = false;
    private _unreadCount = 0;

    constructor(parent: HTMLElement, levelName: string) {
        this._el = document.createElement('div');
        this._el.id = 'game-hud';

        // Candele (sinistra)
        this._candlesEl = document.createElement('span');
        this._candlesEl.className = 'hud-candles';
        this._candlesEl.textContent = '🕯 0';

        // Stato chiave (centro)
        this._keyEl = document.createElement('span');
        this._keyEl.className = 'hud-key';
        this._keyEl.textContent = '';

        // Nome livello + icona diario (destra)
        const rightWrap = document.createElement('span');
        rightWrap.className = 'hud-right';

        this._levelEl = document.createElement('span');
        this._levelEl.className = 'hud-level';
        this._levelEl.textContent = levelName || `LIVELLO`;

        this._diaryBadge = document.createElement('span');
        this._diaryBadge.className = 'hud-diary-badge';
        this._diaryBadge.title = 'Diario [T]';
        this._diaryBadge.textContent = '✦';
        this._diaryBadge.style.display = 'none';
        this._diaryBadge.addEventListener('click', () => this._toggleDiary());

        rightWrap.appendChild(this._levelEl);
        rightWrap.appendChild(this._diaryBadge);

        this._el.appendChild(this._candlesEl);
        this._el.appendChild(this._keyEl);
        this._el.appendChild(rightWrap);
        parent.appendChild(this._el);

        // Diario overlay
        this._diaryOverlay = document.createElement('div');
        this._diaryOverlay.id = 'hud-diary-overlay';
        this._diaryOverlay.style.display = 'none';
        parent.appendChild(this._diaryOverlay);

        // Tasto T / TAB per aprire/chiudere il diario
        document.addEventListener('keydown', this._onKeyDown);
    }

    private readonly _onKeyDown = (e: KeyboardEvent): void => {
        if (e.key === 't' || e.key === 'T' || e.key === 'Tab') {
            e.preventDefault();
            this._toggleDiary();
        }
    };

    private _toggleDiary(): void {
        this._diaryOpen = !this._diaryOpen;
        if (!this._diaryOverlay) return;
        if (this._diaryOpen) {
            this._unreadCount = 0;
            if (this._diaryBadge) this._diaryBadge.style.display = 'none';
            this._diary.forEach(e => { e.read = true; });
            this._renderDiary();
            this._diaryOverlay.style.display = 'flex';
        } else {
            this._diaryOverlay.style.display = 'none';
        }
    }

    private _renderDiary(): void {
        if (!this._diaryOverlay) return;
        this._diaryOverlay.innerHTML = '';

        const header = document.createElement('div');
        header.className = 'diary-header';
        header.textContent = '— DIARIO —';
        this._diaryOverlay.appendChild(header);

        if (this._diary.length === 0) {
            const empty = document.createElement('div');
            empty.className = 'diary-empty';
            empty.textContent = 'Nessun messaggio ancora.';
            this._diaryOverlay.appendChild(empty);
        } else {
            for (const entry of this._diary) {
                const row = document.createElement('div');
                row.className = entry.isEvent ? 'diary-entry diary-entry--event' : 'diary-entry diary-entry--narrative';
                row.textContent = entry.text;
                this._diaryOverlay.appendChild(row);
            }
        }

        const hint = document.createElement('div');
        hint.className = 'diary-hint';
        hint.textContent = '[T] chiudi';
        this._diaryOverlay.appendChild(hint);
    }

    update(candlesLeft: number, _elapsedMs: number, hasKey = false): void {
        this._candlesEl.textContent = `🕯 ${candlesLeft}`;
        if (hasKey) {
            this._keyEl.textContent = '🗝 Trova l\'uscita';
            this._keyEl.style.color = '#ffd700';
        } else {
            this._keyEl.textContent = '';
        }
    }

    showMessage(text: string, color: string, isImportant = false): void {
        // Deduplication: skip se l'ultimo popup attivo ha lo stesso testo
        const last = this._popups[this._popups.length - 1];
        if (last && last.text === text) return;

        // Popup flottante sopra la barra
        let startBottom = POPUP_START_Y_FROM_BOTTOM;
        for (const p of this._popups) {
            const top = p.startBottom + (isImportant ? 42 : 28);
            if (top + 12 > startBottom) startBottom = top + 12;
        }

        const el = document.createElement('div');
        el.className = 'hud-popup' + (isImportant ? ' important' : '');
        el.style.color = color;
        el.style.bottom = `${startBottom}px`;
        el.style.top = 'auto';
        el.style.opacity = '0';
        el.textContent = text;
        this._el.parentElement?.appendChild(el) ?? this._el.appendChild(el);

        this._popups.push({ el, startBottom, elapsed: 0, text });

        // Aggiungi al diario come evento automatico (colore secondario)
        this._addToDiaryInternal(text, 0, true);
    }

    /** Sviluppo A: mostra testo fisso finché il player è sulla cella. */
    showCellText(text: string): void {
        if (this._cellTextActive && this._cellTextEl?.textContent === text) return;
        this._removeCellText();

        this._cellTextEl = document.createElement('div');
        this._cellTextEl.className = 'hud-cell-text';
        this._cellTextEl.textContent = text;
        this._el.parentElement?.appendChild(this._cellTextEl);
        this._cellTextActive = true;
    }

    /** Sviluppo A: rimuove il testo fisso quando il player lascia la cella. */
    hideCellText(): void {
        this._removeCellText();
    }

    private _removeCellText(): void {
        if (this._cellTextEl) {
            this._cellTextEl.remove();
            this._cellTextEl = null;
        }
        this._cellTextActive = false;
    }

    /** Sviluppo B: aggiunge un testo narrativo al diario. */
    addToDiary(text: string, elapsedMs: number): void {
        this._addToDiaryInternal(text, elapsedMs, false);
    }

    private _addToDiaryInternal(text: string, elapsedMs: number, isEvent: boolean): void {
        // Non aggiungere duplicati consecutivi (stesso testo e stesso tipo)
        const last = this._diary[this._diary.length - 1];
        if (last && last.text === text && last.isEvent === isEvent) return;

        this._diary.push({ text, timestamp: elapsedMs, read: false, isEvent });
        this._unreadCount++;

        if (this._diaryBadge) {
            this._diaryBadge.style.display = 'inline';
        }

        if (this._diaryOpen) {
            this._renderDiary();
        }
    }

    get isCellTextActive(): boolean { return this._cellTextActive; }
    get cellTextContent(): string { return this._cellTextEl?.textContent ?? ''; }

    updatePopups(deltaMs: number): void {
        for (let i = this._popups.length - 1; i >= 0; i--) {
            const p = this._popups[i]!;
            p.elapsed += deltaMs;

            const t = Math.min(p.elapsed / POPUP_LIFETIME_MS, 1);
            const alpha = 4 * t * (1 - t);    // parabolic fade-in/out
            const dy    = t * POPUP_FLOAT_PX;  // float upward

            p.el.style.opacity = String(alpha);
            p.el.style.bottom = `${p.startBottom + dy}px`;

            if (p.elapsed >= POPUP_LIFETIME_MS) {
                p.el.remove();
                this._popups.splice(i, 1);
            }
        }
    }

    destroy(): void {
        document.removeEventListener('keydown', this._onKeyDown);
        this._removeCellText();
        this._diaryOverlay?.remove();
        this._el.remove();
        // Rimuovi eventuali popup rimasti
        for (const p of this._popups) p.el.remove();
    }
}
