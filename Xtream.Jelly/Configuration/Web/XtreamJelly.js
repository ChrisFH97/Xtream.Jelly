// Xtream Jelly – Shared utility module for all configuration pages.
// TV-optimized: pagination, search, caching, focus management.

const url = (name) =>
  ApiClient.getUrl("configurationpage", { name });

const tab = (name) => '/configurationpage?name=' + name + '.html';

// Load stylesheet once
$(document).ready(() => {
  if (!document.getElementById('xj-styles')) {
    const style = document.createElement('link');
    style.id = 'xj-styles';
    style.rel = 'stylesheet';
    style.href = url('XtreamJelly.css');
    document.head.appendChild(style);
  }
});

const tabs = [
  { href: tab('XJCredentials'), name: 'Credentials' },
  { href: tab('XJLive'), name: 'Live TV' },
  { href: tab('XJLiveOverrides'), name: 'TV Overrides' },
  { href: tab('XJVod'), name: 'Video On-Demand' },
  { href: tab('XJSeries'), name: 'Series' },
];

const setTabs = (index) => {
  const name = tabs[index].name;
  LibraryMenu.setTabs(name, index, () => tabs);
};

const pluginConfig = {
  UniqueId: 'a1b2c3d4-5678-90ab-cdef-123456789abc'
};

// ===== API Helpers =====
const fetchJson = (apiUrl) => ApiClient.fetch({
  dataType: 'json',
  type: 'GET',
  url: ApiClient.getUrl(apiUrl),
});

const postJson = (apiUrl) => ApiClient.fetch({
  type: 'POST',
  url: ApiClient.getUrl(apiUrl),
});

// ===== Pagination UI Builder =====
// Creates a TV-friendly pagination bar with large buttons
const createPagination = (container, pagedResponse, onPageChange) => {
  container.innerHTML = '';
  if (!pagedResponse || pagedResponse.TotalPages <= 1) return;

  const nav = document.createElement('div');
  nav.className = 'xj-pagination';
  nav.setAttribute('role', 'navigation');
  nav.setAttribute('aria-label', 'Pagination');

  // Previous button
  const prev = document.createElement('button');
  prev.className = 'xj-page-btn';
  prev.innerHTML = '&#9664; Prev';
  prev.disabled = !pagedResponse.HasPreviousPage;
  prev.onclick = () => onPageChange(pagedResponse.Page - 1);
  nav.appendChild(prev);

  // Page numbers (show max 7 around current)
  const total = pagedResponse.TotalPages;
  const current = pagedResponse.Page;
  let start = Math.max(0, current - 3);
  let end = Math.min(total - 1, current + 3);

  if (start > 0) {
    nav.appendChild(createPageBtn(0, current, onPageChange));
    if (start > 1) {
      const dots = document.createElement('span');
      dots.className = 'xj-page-info';
      dots.textContent = '...';
      nav.appendChild(dots);
    }
  }

  for (let i = start; i <= end; i++) {
    nav.appendChild(createPageBtn(i, current, onPageChange));
  }

  if (end < total - 1) {
    if (end < total - 2) {
      const dots = document.createElement('span');
      dots.className = 'xj-page-info';
      dots.textContent = '...';
      nav.appendChild(dots);
    }
    nav.appendChild(createPageBtn(total - 1, current, onPageChange));
  }

  // Page info text
  const info = document.createElement('span');
  info.className = 'xj-page-info';
  const firstItem = current * pagedResponse.PageSize + 1;
  const lastItem = Math.min(firstItem + pagedResponse.PageSize - 1, pagedResponse.TotalCount);
  info.textContent = `${firstItem}-${lastItem} of ${pagedResponse.TotalCount}`;
  nav.appendChild(info);

  // Next button
  const next = document.createElement('button');
  next.className = 'xj-page-btn';
  next.innerHTML = 'Next &#9654;';
  next.disabled = !pagedResponse.HasNextPage;
  next.onclick = () => onPageChange(pagedResponse.Page + 1);
  nav.appendChild(next);

  container.appendChild(nav);
};

const createPageBtn = (page, currentPage, onPageChange) => {
  const btn = document.createElement('button');
  btn.className = 'xj-page-btn' + (page === currentPage ? ' active' : '');
  btn.textContent = page + 1;
  btn.onclick = () => onPageChange(page);
  return btn;
};

// ===== Search Bar Builder =====
const createSearchBar = (container, onSearch) => {
  const bar = document.createElement('div');
  bar.className = 'xj-search-bar';

  const input = document.createElement('input');
  input.type = 'search';
  input.className = 'xj-input';
  input.placeholder = 'Search...';
  input.setAttribute('aria-label', 'Search');

  let debounceTimer;
  input.oninput = () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => onSearch(input.value), 400);
  };

  const clearBtn = document.createElement('button');
  clearBtn.className = 'xj-btn xj-btn-secondary xj-btn-small';
  clearBtn.textContent = 'Clear';
  clearBtn.onclick = () => {
    input.value = '';
    onSearch('');
  };

  bar.appendChild(input);
  bar.appendChild(clearBtn);
  container.appendChild(bar);
  return input;
};

// ===== Item Row Builder =====
const createItemRow = (item, state, update) => {
  const tr = document.createElement('tr');
  tr.dataset['itemId'] = item.Id;

  let td = document.createElement('td');
  const checkbox = document.createElement('input');
  checkbox.type = 'checkbox';
  checkbox.checked = state;
  checkbox.onchange = update;
  checkbox.setAttribute('aria-label', 'Select ' + item.Name);
  td.appendChild(checkbox);
  tr.appendChild(td);

  td = document.createElement('td');
  const label = document.createElement('label');
  label.innerText = item.Name;
  label.style.fontSize = '1.05em';
  td.appendChild(label);
  tr.appendChild(td);

  td = document.createElement('td');
  if (item.HasCatchup) {
    const badge = document.createElement('span');
    badge.className = 'xj-catchup-badge';
    badge.title = `Catch-up: ${item.CatchupDuration} days`;
    badge.innerHTML = `<span class="material-icons" style="font-size:1.1em">timer</span> ${item.CatchupDuration}d`;
    td.appendChild(badge);
  }
  tr.appendChild(td);

  return tr;
};

// ===== Category Row Builder with lazy expand =====
const populateItemsTable = (wrapper, table, items) => {
  for (let i = 0; i < items.length; ++i) {
    const item = items[i];
    const state = wrapper.live !== undefined && (wrapper.live.length === 0 || wrapper.live.includes(item.Id));
    const row = createItemRow(item, state, (e) => {
      let live = wrapper.live;
      if (e.target.checked) {
        live ??= [];
        live.push(item.Id);
        if (items.every(s => live.includes(s.Id))) {
          live = [];
        }
      } else {
        if (live.length === 0) {
          live = items.map(s => s.Id);
        }
        live = live.filter(id => id != item.Id);
        if (live.length === 0) {
          live = undefined;
        }
      }
      wrapper.live = live;
    });
    table.appendChild(row);
  }
};

const setCheckboxState = (checkbox, live) => {
  checkbox.indeterminate = live !== undefined && live.length > 0;
  checkbox.checked = live !== undefined && live.length === 0;
};

const htmlExpand = document.createElement('span');
htmlExpand.ariaHidden = 'true';
htmlExpand.classList.add('material-icons', 'expand_more');

const createCategoryRow = (wrapper, category, loadItems) => {
  const tr = document.createElement('tr');
  tr.dataset['categoryId'] = category.Id;

  let td = document.createElement('td');
  const checkbox = document.createElement('input');
  checkbox.type = 'checkbox';
  setCheckboxState(checkbox, wrapper.live);
  checkbox.setAttribute('aria-label', 'Select category ' + category.Name);
  const onchange = () => {
    if (checkbox.checked) {
      wrapper.live = [];
    } else {
      wrapper.live = undefined;
    }
  };
  checkbox.onchange = onchange;
  td.appendChild(checkbox);
  tr.appendChild(td);

  const _wrapper = {
    get live() { return wrapper.live; },
    set live(value) {
      wrapper.live = value;
      setCheckboxState(checkbox, wrapper.live);
    },
  };

  td = document.createElement('td');
  td.innerHTML = category.Name;
  td.style.fontSize = '1.05em';
  tr.appendChild(td);

  td = document.createElement('td');
  const expand = document.createElement('button');
  expand.type = 'button';
  expand.className = 'xj-expand-btn';
  expand.setAttribute('aria-label', 'Expand ' + category.Name);
  expand.appendChild(htmlExpand.cloneNode(true));

  let expanded = false;
  let itemTable = null;

  expand.onclick = (e) => {
    e.preventDefault();
    if (expanded) {
      expand.firstElementChild.classList.replace('expand_less', 'expand_more');
      if (itemTable) td.removeChild(itemTable);
      itemTable = null;
      expanded = false;
    } else {
      expand.firstElementChild.classList.replace('expand_more', 'expand_less');
      Dashboard.showLoadingMsg();
      itemTable = document.createElement('table');
      itemTable.style.width = '100%';
      loadItems(category.Id).then((items) => {
        populateItemsTable(_wrapper, itemTable, items);
        checkbox.onchange = () => {
          onchange();
          itemTable.querySelectorAll('input[type="checkbox"]').forEach((c) => c.checked = checkbox.checked);
        };
        Dashboard.hideLoadingMsg();
      });
      td.appendChild(itemTable);
      expanded = true;
    }
  };
  td.appendChild(expand);
  tr.appendChild(td);

  return tr;
};

// ===== Paginated Categories Loader =====
const populateCategoriesTable = (table, loadConfig, loadCategories, loadItems) => {
  Dashboard.showLoadingMsg();
  const fetchConfig = loadConfig();
  const fetchCategories = loadCategories();

  return Promise.all([fetchConfig, fetchCategories])
    .then(([config, categories]) => {
      const data = config;
      for (let i = 0; i < categories.length; ++i) {
        const category = categories[i];
        const categoryWrapper = {
          get live() { return data[category.Id]; },
          set live(value) { data[category.Id] = value; },
        };
        const elem = createCategoryRow(categoryWrapper, category, loadItems);
        table.appendChild(elem);
      }
      Dashboard.hideLoadingMsg();
      return data;
    });
};

const filter = (obj, predicate) => Object.keys(obj)
  .filter(key => predicate(obj[key]))
  .reduce((res, key) => (res[key] = obj[key], res), {});

export default {
  createPagination,
  createSearchBar,
  fetchJson,
  filter,
  pluginConfig,
  populateCategoriesTable,
  postJson,
  setTabs,
};
