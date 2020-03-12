const Login = () => {
  if ($('#username').val() == "" || $('#password').val() == "") return;
  $('.login-loading').removeAttr('style');
  AjaxCall({
    url: 'api.asmx/Login',
    param: {
      username: $('#username').val(),
      password: $('#password').val()
    },
    func: (e) => {
      if (e.d != "false") {
        $('body').empty().append(e.d);
      } else {
        $(this).notify('نام کاربری یا رمز عبور اشتباه است', { position: "bottom center", className: "error" });
      }
    }
  });
}

$('#login').on('keydown', function (event) {
  event.keyCode === 13 && Login();
});

$(function () {
  AjaxCall({
    url: 'api.asmx/CheckUserAuthenticated',
    param: {},
    func: (e) => {
      if (e.d != "false") {
        $('body').empty().append(e.d);
      }
    }
  });
});