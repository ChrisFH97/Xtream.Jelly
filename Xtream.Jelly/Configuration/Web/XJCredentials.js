export default function (view) {
  view.addEventListener("viewshow", () => import(
    window.ApiClient.getUrl("web/ConfigurationPage", { name: "XtreamJelly.js" })
  ).then((XJ) => XJ.default
  ).then((XJ) => {
    const pluginId = XJ.pluginConfig.UniqueId;
    XJ.setTabs(0);

    Dashboard.showLoadingMsg();
    ApiClient.getPluginConfiguration(pluginId).then(function (config) {
      view.querySelector('#BaseUrl').value = config.BaseUrl;
      view.querySelector('#Username').value = config.Username;
      view.querySelector('#Password').value = config.Password;
      view.querySelector('#UserAgent').value = config.UserAgent;
      view.querySelector('#PageSize').value = config.PageSize || 20;
      Dashboard.hideLoadingMsg();
    });

    const reloadStatus = () => {
      const fields = {
        status: view.querySelector("#ProviderStatus"),
        expiry: view.querySelector("#ProviderExpiry"),
        cons: view.querySelector("#ProviderConnections"),
        maxCons: view.querySelector("#ProviderMaxConnections"),
        time: view.querySelector("#ProviderTime"),
        timezone: view.querySelector("#ProviderTimezone"),
        mpegTs: view.querySelector("#ProviderMpegTs"),
      };

      XJ.fetchJson('XtreamJelly/TestProvider').then(response => {
        fields.status.innerText = response.Status;
        fields.expiry.innerText = response.ExpiryDate || '—';
        fields.cons.innerText = response.ActiveConnections;
        fields.maxCons.innerText = response.MaxConnections;
        fields.time.innerText = response.ServerTime;
        fields.timezone.innerText = response.ServerTimezone;
        fields.mpegTs.innerText = response.SupportsMpegTs ? 'Yes' : 'No';
      }).catch(() => {
        Object.values(fields).forEach(f => f.innerText = '—');
        fields.status.innerText = "Failed. Check server logs.";
      });
    };
    reloadStatus();

    view.querySelector('#UserAgentFromBrowser').addEventListener('click', (e) => {
      e.preventDefault();
      view.querySelector('#UserAgent').value = navigator.userAgent;
    });

    view.querySelector('#ClearCacheBtn').addEventListener('click', (e) => {
      e.preventDefault();
      XJ.postJson('XtreamJelly/ClearCache').then(() => {
        Dashboard.alert('Cache cleared');
      });
    });

    view.querySelector('#XJCredentialsForm').addEventListener('submit', (e) => {
      Dashboard.showLoadingMsg();
      ApiClient.getPluginConfiguration(pluginId).then((config) => {
        config.BaseUrl = view.querySelector('#BaseUrl').value;
        config.Username = view.querySelector('#Username').value;
        config.Password = view.querySelector('#Password').value;
        config.UserAgent = view.querySelector('#UserAgent').value;
        config.PageSize = parseInt(view.querySelector('#PageSize').value, 10) || 20;
        ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
          reloadStatus();
          Dashboard.processPluginConfigurationUpdateResult(result);
        });
      });
      e.preventDefault();
      return false;
    });
  }));
}
