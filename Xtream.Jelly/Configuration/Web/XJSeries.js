export default function (view) {
  view.addEventListener("viewshow", () => import(
    window.ApiClient.getUrl("web/ConfigurationPage", { name: "XtreamJelly.js" })
  ).then((XJ) => XJ.default
  ).then((XJ) => {
    const pluginId = XJ.pluginConfig.UniqueId;
    XJ.setTabs(4);

    const getConfig = ApiClient.getPluginConfiguration(pluginId);
    const visible = view.querySelector("#Visible");
    getConfig.then((config) => visible.checked = config.IsSeriesVisible);
    const table = view.querySelector('#SeriesContent');
    XJ.populateCategoriesTable(
      table,
      () => getConfig.then((config) => config.Series),
      () => XJ.fetchJson('XtreamJelly/SeriesCategories'),
      (categoryId) => XJ.fetchJson(`XtreamJelly/SeriesCategories/${categoryId}`),
    ).then((data) => {
      view.querySelector('#XJSeriesForm').addEventListener('submit', (e) => {
        Dashboard.showLoadingMsg();

        ApiClient.getPluginConfiguration(pluginId).then((config) => {
          config.IsSeriesVisible = visible.checked;
          config.Series = data;
          ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
            Dashboard.processPluginConfigurationUpdateResult(result);
          });
        });

        e.preventDefault();
        return false;
      });
    }).catch((error) => {
      console.error('Failed to load series categories:', error);
      Dashboard.hideLoadingMsg();
      table.innerHTML = '';
      const errorRow = document.createElement('tr');
      const errorCell = document.createElement('td');
      errorCell.colSpan = 3;
      errorCell.style.color = '#ff6b6b';
      errorCell.style.padding = '16px';
      errorCell.innerHTML = 'Failed to load categories. Please check:<br>' +
        '1. Credentials are configured (Credentials tab)<br>' +
        '2. Xtream server is accessible<br>' +
        '3. Browser console for detailed errors';
      errorRow.appendChild(errorCell);
      table.appendChild(errorRow);
    });
  }));
}
