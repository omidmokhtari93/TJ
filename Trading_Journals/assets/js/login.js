$('#btnLogin').on('click', function () {
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
    
});

$(function() {
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