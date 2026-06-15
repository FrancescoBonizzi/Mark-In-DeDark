import { parseLevel } from '../../grid/GridLoader';
import { serializeMark } from '../../grid/GridSerializer';
import { validateLevel } from '../../grid/LevelValidator';
import type { EditorState } from './editorState';
import { createStateFromBundle, isDirty, markSaved } from './editorState';

/**
 * Apre un file .mark dal filesystem locale.
 * L'input viene appendato al DOM prima del click per evitare GC prematuro
 * (bug: prima apertura silenziosamente ignorata su alcuni browser).
 */
export async function openFile(): Promise<{ state: EditorState; filename: string } | null> {
    return new Promise(resolve => {
        let resolved = false;
        const resolveOnce = (val: { state: EditorState; filename: string } | null) => {
            if (resolved) return;
            resolved = true;
            if (document.body.contains(input)) document.body.removeChild(input);
            resolve(val);
        };

        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.mark';
        input.style.display = 'none';

        input.addEventListener('change', () => {
            const file = input.files?.[0];
            if (!file) { resolveOnce(null); return; }

            const reader = new FileReader();
            reader.onload = () => {
                try {
                    const text = reader.result as string;
                    const bundle = parseLevel(text);
                    const state = createStateFromBundle(bundle, file.name);
                    resolveOnce({ state, filename: file.name });
                } catch (err) {
                    console.error('[LevelEditor] Errore parsing .mark:', err);
                    window.alert(`Errore nel leggere il file:\n${String(err)}`);
                    resolveOnce(null);
                }
            };
            reader.onerror = () => {
                window.alert('Errore nel leggere il file.');
                resolveOnce(null);
            };
            reader.readAsText(file);
        });

        // cancel è non-standard ma supportato in Chrome/Firefox recenti
        input.addEventListener('cancel', () => resolveOnce(null));

        // Appendere al DOM è fondamentale: senza di esso alcuni browser
        // ignorano silenziosamente l'evento change (bug Task 21)
        document.body.appendChild(input);
        input.click();
    });
}

/**
 * Salva il livello corrente come file .mark tramite download.
 * Prima valida: errori → confirm bloccante, warning → confirm.
 * Ritorna true se il salvataggio è avvenuto.
 */
export function saveFile(state: EditorState): boolean {
    const content = serializeMark(state.grid);
    const { errors, warnings } = validateLevel(content);

    if (errors.length > 0) {
        const errList = errors.map(e => `• ${e}`).join('\n');
        const msg =
            `Il livello ha ${errors.length} errore${errors.length > 1 ? 'i' : ''}:\n\n${errList}\n\n` +
            `Salvare comunque?`;
        if (!window.confirm(msg)) return false;
    } else if (warnings.length > 0) {
        const warnList = warnings.map(w => `• ${w}`).join('\n');
        const msg =
            `Il livello ha ${warnings.length} avviso${warnings.length > 1 ? 'i' : ''}:\n\n${warnList}\n\n` +
            `Salvare?`;
        if (!window.confirm(msg)) return false;
    }

    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = state.currentFilename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    markSaved(state);
    return true;
}

/**
 * Mostra confirm se ci sono modifiche non salvate.
 */
export function confirmIfDirty(state: EditorState): boolean {
    if (!isDirty(state)) return true;
    return window.confirm('Ci sono modifiche non salvate. Procedere senza salvare?');
}
