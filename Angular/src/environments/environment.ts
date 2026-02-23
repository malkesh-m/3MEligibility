
export const environment = {
    apiUrl: 'https://localhost:50647/api',
   production: false,
  identitySettings: {
    authority: 'https://auth.3m-technology.com',
    clientId: '3MEligibility-Client',
    scope: 'profile openid email',
    authorizedUris: ['http://localhost:4200'],
    loginCallBackPath: 'http://localhost:4200',
    logoutCallBackPath: 'http://localhost:4200',
    loginCallBackPage: 'login-callback',
    logoutCallBackPage: 'logout-callback',
  },
  server: {
    apiUrl: 'https://localhost:50647/',
  },
    identityAccountUrl: {
    Url: 'https://account.3m-technology.com/',
  },
};

// export const keycloakConfig = {
//   authority: 'https://auth.3m-technology.com',
//   client_id: '3MEligibility-Client',
//   redirect_uri: window.location.origin + '/authentication/login-callback',
//   post_logout_redirect_uri: window.location.origin, // For logout
//   response_type: 'code',
//   scope: 'profile openid email',
//   automaticSilentRenew: true,
//   loadUserInfo: true,
 
 
// };
 