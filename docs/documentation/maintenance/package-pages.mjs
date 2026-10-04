import { readdir, readFile, writeFile, mkdir, cp, rm, access } from 'node:fs/promises';
import { resolve, dirname, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const documents = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const project = resolve(documents, '../..');
const output = resolve(project, '.pages-site');
const repository = 'https://github.com/MDJ0126/FrameworkForUnity/blob/main/';

// Delete only this dedicated generated directory inside the project.
if (relative(project, output) !== '.pages-site') throw new Error('Invalid Pages output directory');
await rm(output, { recursive: true, force: true });
await mkdir(output);
for (const name of await readdir(documents)) {
  if (name.endsWith('.html')) {
    const html = (await readFile(resolve(documents, name), 'utf8'))
      .replace(/href=(\\?")\.\.\/\.\.\//g, (_, quote) => 'href=' + quote + repository)
      .replace(/src=(\\?")\.\.\/images\//g, (_, quote) => 'src=' + quote + 'images/');
    await writeFile(resolve(output, name), html);
  } else if (name.endsWith('.md') || name === 'favicon.svg') {
    await cp(resolve(documents, name), resolve(output, name));
  }
}
await cp(resolve(documents, 'vendor'), resolve(output, 'vendor'), { recursive: true });
await mkdir(resolve(output, 'images'));
await cp(resolve(project, 'docs/images/thumbnail.png'), resolve(output, 'images/thumbnail.png'));
await writeFile(resolve(output, '.nojekyll'), '');

// Verify the actual staged links, including paths rewritten inside embedded views.
let links = 0;
for (const name of (await readdir(output)).filter(name => name.endsWith('.html'))) {
  const html = await readFile(resolve(output, name), 'utf8');
  if (/\b(?:href|src)=(?:\\?")\.\.\//.test(html)) throw new Error(`Unpackaged project link: ${name}`);
  for (const match of html.matchAll(/\b(?:href|src)="([^"#]+)"/g)) {
    const href = match[1].replaceAll('&amp;', '&');
    if (/^[a-z]+:/i.test(href)) continue;
    await access(resolve(output, href.split('#')[0]));
    links++;
  }
}
console.log(`Prepared .pages-site; ${links} local links verified. Live deployment is not checked.`);

// Keep the existing public documentation URL working after Actions deployment.
const siteEntries = await readdir(output);
const documentAlias = resolve(output, 'docs/documentation');
await mkdir(documentAlias, { recursive: true });
for (const name of siteEntries) {
  await cp(resolve(output, name), resolve(documentAlias, name), { recursive: true });
}
await access(resolve(documentAlias, 'index.html'));
