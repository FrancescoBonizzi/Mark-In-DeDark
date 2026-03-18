import stringHelper from "./StringHelper.ts";

const ScorePrefix = 'mark-in-dedark-';

const makeKey = (levelId: number, suffix: string) => `${ScorePrefix}level-${levelId}-${suffix}`;

const toNumberOr0 = (value: string | null | undefined): number => {
    if (stringHelper.isNullOrWhitespace(value)) {
        return 0;
    }

    const number = Number(value);
    if (isNaN(number)) {
        return 0;
    }

    return number;
};

export default {
    isLevelCompleted: (levelId: number): boolean => {
        return localStorage.getItem(makeKey(levelId, 'completed')) === '1';
    },

    setLevelCompleted: (levelId: number): void => {
        localStorage.setItem(makeKey(levelId, 'completed'), '1');
    },

    getLevelTime: (levelId: number): number => {
        return toNumberOr0(localStorage.getItem(makeKey(levelId, 'time')));
    },

    setLevelTime: (levelId: number, value: number): void => {
        localStorage.setItem(makeKey(levelId, 'time'), value.toString());
    },

    getLevelCandles: (levelId: number): number => {
        return toNumberOr0(localStorage.getItem(makeKey(levelId, 'candles')));
    },

    setLevelCandles: (levelId: number, value: number): void => {
        localStorage.setItem(makeKey(levelId, 'candles'), value.toString());
    },
};
