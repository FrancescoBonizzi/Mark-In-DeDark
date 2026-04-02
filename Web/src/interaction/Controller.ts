import {CharacterActions} from "./CharacterActions.ts";

// Actions that must fire once per keypress, not every frame while held.
const ONE_SHOT_ACTIONS = new Set<CharacterActions>([
    CharacterActions.Candle,
    CharacterActions.DoorSwitch,
    CharacterActions.GameBack,
]);

const KEY_MAP: Record<string, CharacterActions> = {
    'ArrowUp': CharacterActions.Up,
    'ArrowDown': CharacterActions.Down,
    'ArrowLeft': CharacterActions.Left,
    'ArrowRight': CharacterActions.Right,
    'w': CharacterActions.Up,
    'W': CharacterActions.Up,
    's': CharacterActions.Down,
    'S': CharacterActions.Down,
    'a': CharacterActions.Left,
    'A': CharacterActions.Left,
    'd': CharacterActions.Right,
    'D': CharacterActions.Right,
    ' ': CharacterActions.Candle,
    'Control': CharacterActions.DoorSwitch,
    'Escape': CharacterActions.GameBack,
};

const DIRECTION_TO_IDLE: Partial<Record<CharacterActions, CharacterActions>> = {
    [CharacterActions.Up]: CharacterActions.IdleUp,
    [CharacterActions.Down]: CharacterActions.IdleDown,
    [CharacterActions.Left]: CharacterActions.IdleLeft,
    [CharacterActions.Right]: CharacterActions.IdleRight,
};

class Controller {
    private _currentAction: CharacterActions = CharacterActions.Idle;
    private _lastDirection: CharacterActions = CharacterActions.Down;
    private _pressedKeys = new Set<string>();
    // Keys whose one-shot action has already been reported this press cycle.
    private _reportedOneShotKeys = new Set<string>();

    constructor(target: EventTarget = window) {
        target.addEventListener('keydown', this.onKeyDown as EventListener);
        target.addEventListener('keyup', this.onKeyUp as EventListener);
        window.addEventListener('blur', this.reset);
        document.addEventListener('visibilitychange', this.reset);
    }

    private onKeyDown = (e: KeyboardEvent) => {
        this._pressedKeys.add(e.key);
        this.updateAction();
    };

    private onKeyUp = (e: KeyboardEvent) => {
        this._pressedKeys.delete(e.key);
        this._reportedOneShotKeys.delete(e.key);
        this.updateAction();
    };

    private reset = () => {
        this._pressedKeys.clear();
        this._reportedOneShotKeys.clear();
        this._currentAction = DIRECTION_TO_IDLE[this._lastDirection] ?? CharacterActions.Idle;
    };

    private updateAction() {
        // Check directions first (priority: last pressed wins via set iteration order)
        for (const key of this._pressedKeys) {
            const action = KEY_MAP[key];
            if (action !== undefined && action <= CharacterActions.Left) {
                this._currentAction = action;
                this._lastDirection = action;
                return;
            }
        }

        // Check action keys
        for (const key of this._pressedKeys) {
            const action = KEY_MAP[key];
            if (action !== undefined) {
                this._currentAction = action;
                return;
            }
        }

        // No keys pressed — idle in last direction
        this._currentAction = DIRECTION_TO_IDLE[this._lastDirection] ?? CharacterActions.Idle;
    }

    get currentAction(): CharacterActions {
        const action = this._currentAction;
        if (ONE_SHOT_ACTIONS.has(action)) {
            // Find the key responsible and check if already reported this press.
            for (const key of this._pressedKeys) {
                if (KEY_MAP[key] === action) {
                    if (this._reportedOneShotKeys.has(key)) {
                        return DIRECTION_TO_IDLE[this._lastDirection] ?? CharacterActions.Idle;
                    }
                    this._reportedOneShotKeys.add(key);
                    return action;
                }
            }
        }
        return action;
    }

    dispose() {
        window.removeEventListener('keydown', this.onKeyDown as EventListener);
        window.removeEventListener('keyup', this.onKeyUp as EventListener);
        window.removeEventListener('blur', this.reset);
        document.removeEventListener('visibilitychange', this.reset);
    }
}

export default Controller;
