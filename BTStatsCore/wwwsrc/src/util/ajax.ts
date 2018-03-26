import {ajax} from 'jquery';

export function get(endpoint: string) {
  return new Promise((resolve, reject) => {
    ajax({
      url: endpoint,
      type: 'GET',
      contentType: 'application/json',
      dataType: 'json',
      global: false,
      complete: (response, status) => {
        if (status !== 'success') {
          reject();
        }
        resolve({
          data: response.responseJSON,
        });
      },
    })
  });
}
