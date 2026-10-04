import { readFile } from 'node:fs/promises';
import { Script, createContext } from 'node:vm';
import assert from 'node:assert/strict';

const html = await readFile(new URL('../index.html', import.meta.url), 'utf8');
const script = html.match(/<script>([\s\S]*?)<\/script>/)[1];
const handlers = {};
const article = { id: 'chapter-0', innerHTML: '' };
const breadcrumb = { textContent: '' }, pager = { innerHTML: '' };
const links = [...html.matchAll(/<a href="([^"]+)" data-chapter="([^"]+)"/g)].map(match => ({
  href: match[1], dataset: { chapter: match[2] }, attributes: {},
  setAttribute(name, value) { this.attributes[name] = value; },
  removeAttribute(name) { delete this.attributes[name]; },
  getAttribute(name) { return name === 'href' ? this.href : this.attributes[name]; },
  hasAttribute(name) { return name in this.attributes; },
}));
const results = { innerHTML: '', hidden: true }, empty = { hidden: true };
const input = { value: '', addEventListener(name, handler) { handlers[name] = handler; } };
const document = {
  title: '', getElementById: id => ({ search: input, 'script-results': results, empty })[id],
  querySelector: selector => ({ article, '.breadcrumb': breadcrumb, '.pager': pager })[selector],
  querySelectorAll: selector => selector === '[data-chapter]' ? links : [],
  addEventListener: (name, handler) => { handlers[name] = handler; },
};
const location = { hash: '' };
const window = { addEventListener: (name, handler) => { handlers[name] = handler; }, scrollTo() {} };
const context = createContext({ document, window, location });
new Script(script).runInContext(context);
const crossToken = new Script(`underlineMatches('<span class="cs-identifier">springArm</span>.<span class="cs-type">CameraWorldPosition</span>,', ['m.cameraworldposition,'])`).runInContext(context);
assert(crossToken.includes('springAr<u class="search-match">m</u>'));
assert(crossToken.includes('<u class="search-match">.</u>'));
assert(crossToken.includes('<u class="search-match">CameraWorldPosition</u>'));
assert(crossToken.includes('<u class="search-match">,</u>'));
const fuzzyUnderline = new Script(`underlineMatches('<span class="cs-type">FollowPawnInfo</span>', ['followinfo'])`).runInContext(context);
assert(fuzzyUnderline.includes('<u class="search-match">Follow</u>Pawn<u class="search-match">Info</u>'));
assert.equal(new Script(`searchSelection('public class FollowPawnInfo', ['followinfo']).some(Boolean)`).runInContext(context), true);
const classLink = links.find(link => link.href === '12-class.html');
let prevented = false;
handlers.click({ button: 0, target: { closest: () => classLink }, preventDefault() { prevented = true; } });
assert(prevented, 'Navigation must prevent a full page load');
assert(article.innerHTML.includes('Class 작성'));
assert.equal(location.hash, '#' + classLink.dataset.chapter);
assert.equal(classLink.attributes['aria-current'], 'page');
assert(breadcrumb.textContent.includes('기본 작성'));
assert(pager.innerHTML.includes('13-struct.html'));
location.hash = '';
handlers.hashchange();
assert.equal(article.id, 'chapter-0');
const skillLink = links.find(link => link.href === '15-skill.html');
prevented = false;
handlers.click({ button: 0, ctrlKey: true, target: { closest: () => skillLink }, preventDefault() { prevented = true; } });
assert.equal(prevented, false, 'Modified clicks must preserve opening another tab');
input.value = 'Character';
input.value = 'FollowInfo';
handlers.input();
assert(results.innerHTML.includes('FollowPawnInfo.cs'));
input.value = 'followinfo';
handlers.input();
assert(results.innerHTML.includes('FollowPawnInfo.cs'));
input.value = 'InfoFollow';
handlers.input();
assert(!results.innerHTML.includes('<strong>FollowPawnInfo.cs</strong>'));
input.value = 'Character';
handlers.input();
assert.equal(results.hidden, false);
assert(results.innerHTML.includes('Character.cs'));
const href = results.innerHTML.match(/href="([^"]*%2FCharacter\.cs)"/)[1];
const sourceLink = { getAttribute: () => href, hasAttribute: () => false };
prevented = false;
handlers.click({ button: 0, target: { closest: () => sourceLink }, preventDefault() { prevented = true; } });
assert(prevented);
assert.equal(article.id, 'script-view');
assert(article.innerHTML.includes('line-number'));
assert(article.innerHTML.replace(/<[^>]+>/g, '').includes('class Character'));
assert(article.innerHTML.includes('class="cs-keyword"'));
assert(article.innerHTML.includes('class="cs-type"'));
assert(breadcrumb.textContent.includes('Assets/Scripts/'));
assert.equal(pager.innerHTML, '');
assert(article.innerHTML.includes('class="search-match"'));
const scriptLocation = location.hash;
input.value = 'public';
handlers.input();
assert(article.innerHTML.includes('<u class="search-match">public</u>'));
assert.equal(location.hash, scriptLocation, 'Typing must keep the current script open');
input.value = '';
handlers.input();
assert(!article.innerHTML.includes('class="search-match"'));
assert(!article.innerHTML.includes('code-row matched'));
assert(html.includes('<a class="brand" href="index.html"'));
const homeLink = { getAttribute: () => 'index.html', hasAttribute: () => false };
prevented = false;
handlers.click({ button: 0, target: { closest: () => homeLink }, preventDefault() { prevented = true; } });
assert(prevented, 'Brand must navigate home without reloading');
assert.equal(article.id, 'chapter-0');
assert(article.innerHTML.includes('<img class="document-image" src="../images/thumbnail.png"'));
location.hash = '#' + classLink.dataset.chapter;
handlers.hashchange();
assert(article.innerHTML.includes('Class 작성'));
input.value = 'no-result-unique-938524';
handlers.input();
assert.equal(empty.hidden, false);
input.value = '';
handlers.input();
assert.equal(results.hidden, true);
assert.equal(empty.hidden, true);
console.log('PASS: in-page navigation prevents reload; active link, breadcrumb, pager, back navigation and modified clicks verified.');
console.log('PASS: script search, source viewer, line numbers, history, no results and clearing verified.');
