function CheckRequiredFields(n) {
    var flag = false;
    var elements = $("#" + n).find("input[required] ,textarea[required] ,select[required]");
    for (var i = 0; i < elements.length; i++) {
        if ($(elements[i]).val() === '') {
            $(elements[i]).css('border' , '1px solid red');
            flag = true;
        }
    }
    setTimeout(function () { $(elements).css("border" , ''); }, 5000);
    return flag;
}

function RedAlert(ele, txt) {
    var element;
    if (typeof ele == "string") { element = $("#" + ele); } else { element = ele; }
    $(element).addClass('form-controlError');
    setTimeout(function () { $(element).removeClass("form-controlError"); }, 4000);
    $.notify(txt, { globalPosition: 'top left' });
}

function GreenAlert(ele, txt) {
    var element;
    if (typeof ele == "string") { element = $("#" + ele); } else { element = ele; }
    $(element).css('background-color', 'lightgreen');
    setTimeout(function () { $(element).removeAttr('style'); }, 4000);
    $.notify(txt, { className: 'success', clickToHide: false, autoHide: true, position: 'top left' });
}

function AjaxCall(obj) {
    $.ajax({
        type: 'POST',
        url: obj.url,
        data: JSON.stringify(obj.param),
        contentType: 'application/json;',
        dataType: 'json',
        success: obj.func,
        error: function () {
            RedAlert('n', 'خطا در برقراری ارتباط با سرور');
        }
    });
}