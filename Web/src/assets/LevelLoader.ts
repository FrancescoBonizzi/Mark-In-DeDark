import { AssetsRoot } from './constants';

export const loadLevelText = async (folder: string, levelId: number): Promise<string> => {
    const url = `${AssetsRoot}assets/levels/${folder}/${levelId}.mark`;
    const response = await fetch(url);
    if (!response.ok) throw new Error(`Failed to load level ${folder}/${levelId}`);
    return response.text();
};
