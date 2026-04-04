import stringHelper from "./StringHelper.ts";

const ScorePrefix = 'mark-in-dedark-';

const makeKey = (folder: string, levelId: number, suffix: string) => `${ScorePrefix}${folder}-level-${levelId}-${suffix}`;

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
    isLevelCompleted: (folder: string, levelId: number): boolean => {
        return localStorage.getItem(makeKey(folder, levelId, 'completed')) === '1';
    },

    setLevelCompleted: (folder: string, levelId: number): void => {
        localStorage.setItem(makeKey(folder, levelId, 'completed'), '1');
    },

    getLevelTime: (folder: string, levelId: number): number => {
        return toNumberOr0(localStorage.getItem(makeKey(folder, levelId, 'time')));
    },

    setLevelTime: (folder: string, levelId: number, value: number): void => {
        localStorage.setItem(makeKey(folder, levelId, 'time'), value.toString());
    },

    getLevelCandles: (folder: string, levelId: number): number => {
        return toNumberOr0(localStorage.getItem(makeKey(folder, levelId, 'candles')));
    },

    setLevelCandles: (folder: string, levelId: number, value: number): void => {
        localStorage.setItem(makeKey(folder, levelId, 'candles'), value.toString());
    },
};
