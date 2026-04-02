import { Container, Spritesheet } from 'pixi.js';
import { Enemy } from '../Enemy';
import { Zombie } from './Zombie';
import { Skeleton } from './Skeleton';
import { GhostEnemy } from './GhostEnemy';
import { Goblin } from './Goblin';
import { Taurus } from './Taurus';
import { Bubu } from './Bubu';
import { GameGrid } from '../../grid/GameGrid';
import { GameCell, CELL_SIZE } from '../../grid/GameCell';
import { ObjectTypes } from '../../grid/enums';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { ShadowSystem } from '../../lighting/ShadowSystem';

/**
 * Letter prefix used in the level file's patrol waypoint codes.
 * Format: "{letter}-{patrolNum}-{waypointIndex}"
 * Z=Zombie, S=Skeleton, T=Taurus, G=Ghost, B=Bubu
 */
const LETTER_TO_TYPE: Record<string, ObjectTypes> = {
    Z: ObjectTypes.Zombie,
    S: ObjectTypes.Scheletro,
    T: ObjectTypes.Taurus,
    G: ObjectTypes.Ghost,
    B: ObjectTypes.Bubu,
};

export function createEnemiesFromGrid(
    grid: GameGrid,
    sheets: Spritesheet[],
    objectsContainer: Container,
    objectsNormalContainer: Container,
    player: Player,
    candlesManager: CandlesManager,
    shadowSystem: ShadowSystem,
    onShake?: (intensity: number) => void,
): Enemy[] {
    const enemies: Enemy[] = [];

    // --- Patrol-based enemies (Zombie, Skeleton, Taurus, Ghost, Bubu) ---
    // Each PatrolPath entry corresponds to one enemy's full patrol route.
    // Waypoint 0 is the spawn position.
    for (const path of grid.patrolPaths) {
        const objectType = LETTER_TO_TYPE[path.letter];
        if (objectType === undefined) continue;

        const waypoints = path.waypoints
            .map(w => grid.getCell(w.col, w.row))
            .filter((c): c is GameCell => c !== null);

        if (waypoints.length === 0) continue;

        const spawnCell = waypoints[0]!;
        const startX = spawnCell.centerX;
        const startY = spawnCell.centerY;

        let enemy: Enemy | null = null;

        switch (objectType) {
            case ObjectTypes.Zombie:
                enemy = new Zombie(
                    grid, sheets, objectsContainer, objectsNormalContainer,
                    startX, startY, waypoints, player, candlesManager,
                );
                break;
            case ObjectTypes.Scheletro:
                enemy = new Skeleton(
                    grid, sheets, objectsContainer, objectsNormalContainer,
                    startX, startY, waypoints, player, candlesManager,
                );
                break;
            case ObjectTypes.Taurus:
                enemy = new Taurus(
                    grid, sheets, objectsContainer, objectsNormalContainer,
                    startX, startY, waypoints, player, candlesManager, onShake,
                );
                break;
            case ObjectTypes.Ghost:
                enemy = new GhostEnemy(
                    grid, sheets, objectsContainer, objectsNormalContainer,
                    startX, startY, waypoints, player, candlesManager,
                );
                break;
            case ObjectTypes.Bubu:
                enemy = new Bubu(
                    grid, sheets, objectsContainer, objectsNormalContainer,
                    startX, startY, waypoints, player, candlesManager,
                );
                break;
        }

        if (enemy) {
            if (enemy.surroundingLight) shadowSystem.addLight(enemy.surroundingLight);
            enemies.push(enemy);
        }
    }

    // --- Goblin (no patrol path — spawn at placement cell, hunts player) ---
    for (const placement of grid.enemyPlacements) {
        if (placement.objectType !== ObjectTypes.Goblin) continue;

        const startX = placement.col * CELL_SIZE + CELL_SIZE / 2;
        const startY = placement.row * CELL_SIZE + CELL_SIZE / 2;

        const goblin = new Goblin(
            grid, sheets, objectsContainer, objectsNormalContainer,
            startX, startY, player, candlesManager,
        );
        if (goblin.surroundingLight) shadowSystem.addLight(goblin.surroundingLight);
        enemies.push(goblin);
    }

    return enemies;
}
