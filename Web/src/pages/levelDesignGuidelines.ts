import { LEVEL_DESIGN_GUIDELINES_MD } from "../generatedLevelDesignGuidelines";
import "./levelDesignGuidelines.css";

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function renderInlineMarkdown(text: string): string {
  let html = escapeHtml(text);
  html = html.replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>");
  html = html.replace(/\*(.+?)\*/g, "<em>$1</em>");
  html = html.replace(/`(.+?)`/g, "<code>$1</code>");
  return html;
}

function markdownToHtml(markdown: string): string {
  const lines = markdown.replace(/\r\n/g, "\n").split("\n");
  const out: string[] = [];

  let inUl = false;
  let inOl = false;
  let paragraph: string[] = [];

  function flushParagraph(): void {
    if (paragraph.length === 0) return;
    out.push(`<p>${renderInlineMarkdown(paragraph.join(" "))}</p>`);
    paragraph = [];
  }

  function closeLists(): void {
    if (inUl) {
      out.push("</ul>");
      inUl = false;
    }
    if (inOl) {
      out.push("</ol>");
      inOl = false;
    }
  }

  for (const rawLine of lines) {
    const line = rawLine.trim();

    if (line.length === 0) {
      flushParagraph();
      closeLists();
      continue;
    }

    if (/^---+$/.test(line)) {
      flushParagraph();
      closeLists();
      out.push("<hr />");
      continue;
    }

    const heading = line.match(/^(#{1,3})\s+(.+)$/);
    if (heading) {
      flushParagraph();
      closeLists();
      const level = heading[1]!.length;
      out.push(`<h${level}>${renderInlineMarkdown(heading[2]!)}</h${level}>`);
      continue;
    }

    const ordered = line.match(/^\d+\.\s+(.+)$/);
    if (ordered) {
      flushParagraph();
      if (!inOl) {
        if (inUl) {
          out.push("</ul>");
          inUl = false;
        }
        out.push("<ol>");
        inOl = true;
      }
      out.push(`<li>${renderInlineMarkdown(ordered[1]!)}</li>`);
      continue;
    }

    const unordered = line.match(/^-\s+(.+)$/);
    if (unordered) {
      flushParagraph();
      if (!inUl) {
        if (inOl) {
          out.push("</ol>");
          inOl = false;
        }
        out.push("<ul>");
        inUl = true;
      }
      out.push(`<li>${renderInlineMarkdown(unordered[1]!)}</li>`);
      continue;
    }

    closeLists();
    paragraph.push(line);
  }

  flushParagraph();
  closeLists();

  return out.join("\n");
}

export function renderLevelDesignGuidelinesPage(container: HTMLElement): void {
  const guidelinesHtml = markdownToHtml(LEVEL_DESIGN_GUIDELINES_MD);

  container.innerHTML = `
    <div class="guidelines-page">
      <header class="guidelines-header">
        <a class="guidelines-back" href="#/level-editor">← Torna al Level Editor</a>
      </header>
      <main class="guidelines-content">${guidelinesHtml}</main>
    </div>
  `;
}
