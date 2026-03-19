export interface CollisionOffset {
    x: number;
    y: number;
    w: number;
    h: number;
}

export interface LevelCustomization {
    wallFixedVerticalOffset: CollisionOffset;
    wallFixedHorizontalOffset: CollisionOffset;
    pillarOffset: CollisionOffset;
}

export function createLevelCustomization(levelId: number): LevelCustomization {
    const defaultPillar: CollisionOffset = { x: 16, y: 144, w: 48, h: 32 };
    const smallPillar: CollisionOffset = { x: 22, y: 144, w: 40, h: 24 };

    return {
        wallFixedVerticalOffset: { x: 12, y: 96, w: 40, h: 192 },
        wallFixedHorizontalOffset: { x: 0, y: 136, w: 192, h: 32 },
        pillarOffset: levelId <= 2 ? smallPillar : defaultPillar,
    };
}
