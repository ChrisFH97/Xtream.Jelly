export default function (view) {
  const createChannelRow = (channel, overrides) => {
    const tr = document.createElement('tr');
    tr.dataset['channelId'] = channel.Id;

    let td = document.createElement('td');
    const number = document.createElement('input');
    number.type = 'number';
    number.className = 'xj-input';
    number.placeholder = channel.Number;
    number.value = overrides.Number ?? '';
    number.onchange = () => number.value ?
      overrides.Number = parseInt(number.value) :
      delete overrides.Number;
    td.appendChild(number);
    tr.appendChild(td);

    td = document.createElement('td');
    const name = document.createElement('input');
    name.type = 'text';
    name.className = 'xj-input';
    name.placeholder = channel.Name;
    name.value = overrides.Name ?? '';
    name.onchange = () => name.value ?
      overrides.Name = name.value :
      delete overrides.Name;
    td.appendChild(name);
    tr.appendChild(td);

    td = document.createElement('td');
    const image = document.createElement('input');
    image.type = 'text';
    image.className = 'xj-input';
    image.placeholder = channel.LogoUrl ?? '';
    image.value = overrides.LogoUrl ?? '';
    image.onchange = () => image.value ?
      overrides.LogoUrl = image.value :
      delete overrides.LogoUrl;
    td.appendChild(image);
    tr.appendChild(td);

    return tr;
  };

  view.addEventListener("viewshow", () => import(
    window.ApiClient.getUrl("web/ConfigurationPage", { name: "XtreamJelly.js" })
  ).then((XJ) => XJ.default
  ).then((XJ) => {
    const pluginId = XJ.pluginConfig.UniqueId;
    XJ.setTabs(2);

    const getConfig = ApiClient.getPluginConfiguration(pluginId);
    const table = view.querySelector('#LiveChannels');
    Dashboard.showLoadingMsg();
    Promise.all([
      getConfig.then((config) => config.LiveTvOverrides),
      XJ.fetchJson('XtreamJelly/LiveTv'),
    ]).then(([data, pagedResponse]) => {
      const channels = pagedResponse.Items || pagedResponse;
      for (const channel of channels) {
        data[channel.Id] ??= {};
        const row = createChannelRow(channel, data[channel.Id]);
        table.appendChild(row);
      }
      Dashboard.hideLoadingMsg();

      view.querySelector('#XJLiveOverridesForm').addEventListener('submit', (e) => {
        Dashboard.showLoadingMsg();

        ApiClient.getPluginConfiguration(pluginId).then((config) => {
          config.LiveTvOverrides = XJ.filter(
            data,
            overrides => Object.keys(overrides).length > 0
          );
          ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
            Dashboard.processPluginConfigurationUpdateResult(result);
          });
        });

        e.preventDefault();
        return false;
      });
    });
  }));
}
