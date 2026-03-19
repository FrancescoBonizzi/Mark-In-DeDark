import { AssetsRoot } from './constants';

export const loadLevelText = async (levelId: number): Promise<string> => {
    const url = `${AssetsRoot}assets/levels/${levelId}.mark`;
    const response = await fetch(url);
    if (!response.ok) throw new Error(`Failed to load level ${levelId}`);
    return response.text();
};
