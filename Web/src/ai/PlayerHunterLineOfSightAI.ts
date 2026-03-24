import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { Player } from '../characters/Player';
import type { CandlesManager } from '../objects/CandlesManager';
import type { PatrolAI } from './PatrolAI';
import { KillCandleAI } from './KillCandleAI';
import { checkLineOfSight } from './LineOfSight';

/** Speed (ms/cell) used when hunting (Skeleton spec from GameConfig). */
const HUNTING_SPEED_MS = 500;
/** Rage tint applied to the enemy light while hunting. */
const RAGE_COLOR = 0xFF0000;

const INCREDULITY_PHASE_MS = 500;
const INCREDULITY_PHASES = 6;

const enum HuntStatus { Following, Incredulity, Lost }

/**
 * 3-phase hunter AI active after the Skeleton spots the player:
 * Following → Incredulity (confusion look) → Lost (return to patrol).
 * (port of C# PlayerHunterLineOfSightAI)
 */
export class PlayerHunterLineOfSightAI implements IEnemyAI {
    private readonly _player: Player;
    private readonly _grid: GameGrid;
    private readonly _candlesManager: CandlesManager;
    private readonly _patrolAI: PatrolAI;
    private _actions: CharacterActions[];
    private _status = HuntStatus.Following;
    private _elapsed = 0;
    private _incredulityPhase = 0;
    private readonly _originalSpeedMS: number;

    constructor(
        enemy: Enemy,
        patrolAI: PatrolAI,
        firstPath: CharacterActions[],
        grid: GameGrid,
        player: Player,
        candlesManager: CandlesManager,
    ) {
        this._player = player;
        this._grid = grid;
        this._candlesManager = candlesManager;
        this._patrolAI = patrolAI;
        this._actions = [...firstPath];
        this._originalSpeedMS = enemy.movementSpeedMS;

        // Speed up and tint
        enemy.movementSpeedMS = HUNTING_SPEED_MS;
        enemy.surroundingLight?.setTemporaryColor(RAGE_COLOR);
    }

    update(enemy: Enemy, deltaMS: number): CharacterActions {
        switch (this._status) {
            case HuntStatus.Following: {
                // Candle check first
                const cell = enemy.currentCell;
                if (cell && enemy.nCellsCandleRange !== null) {
                    const result = this._candlesManager.getNearestCandlePath(cell, enemy.nCellsCandleRange, this._grid);
                    if (result) {
                        return enemy.setNewAI(new KillCandleAI(result.candle, result.actions, this));
                    }
                }

                // Re-check LoS
                const losPath = checkLineOfSight(enemy, this._player.worldCenter.x, this._player.worldCenter.y, this._grid);
                if (losPath !== null) {
                    this._actions = losPath;
                } else {
                    // LoS lost — switch to incredulity
                    this._status = HuntStatus.Incredulity;
                    this._elapsed = 0;
                    this._incredulityPhase = 0;
                    return CharacterActions.IdleLeft;
                }

                if (this._actions.length > 0) return this._actions.shift()!;
                return CharacterActions.Idle;
            }

            case HuntStatus.Incredulity: {
                this._elapsed += deltaMS;
                if (this._elapsed >= INCREDULITY_PHASE_MS) {
                    this._elapsed = 0;
                    this._incredulityPhase++;
                    if (this._incredulityPhase >= INCREDULITY_PHASES) {
                        this._status = HuntStatus.Lost;
                    }
                }
                return this._incredulityPhase % 2 === 0 ? CharacterActions.IdleLeft : CharacterActions.IdleRight;
            }

            case HuntStatus.Lost: {
                // Restore speed and color
                enemy.movementSpeedMS = this._originalSpeedMS;
                enemy.surroundingLight?.resetColor();
                this._patrolAI.clearActions();
                return enemy.setNewAI(this._patrolAI);
            }
        }
    }
}
