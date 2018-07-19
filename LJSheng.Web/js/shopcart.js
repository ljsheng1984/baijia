$.ajax({
    url: "/Home/ClassifyList",
    data: "",
    type: "post",
    cache: false,
    timeout: 8000,
    dataType: "json",
    ContentType: "application/json; charset=utf-8",
    error: function () { },
    beforeSend: function () { },
    success: function (data) {
        //console.log(data);
        if (data.result == 200) {
            $.each(data.data.list, function (index, content) {
                var c = "";
                if (index == 0) {
                    c = " class=\"active\"";
                    $("input[name='number']").val(index);
                    $("input[name='ClassifyGid']").val(content.Gid);
                }
                $("#menu").append("<li id=\"menu" + index + "\"><a onclick=\"list('" + index + "','" + content.Gid + "','1');\"" + c + ">" + content.Name + "</a></li>");
                $("#list").append("<ul class=\"index-groom\" id=\"list" + index + "\"></ul>");
            });
            list($("input[name='number']").val(), $("input[name='ClassifyGid']").val(), 1);
        }
    },
});
function list(number, gid, page) {
    $("#menu li a").removeClass("active");
    $("#menu" + number + " a").addClass("active");
    $("#list ul").hide();
    $("#list" + number).show();
    if (page == 1) {
        $("#list" + number).html("");
        $("input[name='pageindex']").val("1");
        $("input[name=number]").val(number);
        $("input[name=ClassifyGid]").val(gid);
    }
    else {
        var pi = parseInt($("input[name='pageindex']").val());
        $("input[name='pageindex']").val(pi++);
    }
    var json = "{";
    try {
        var div = document.getElementById("search");
        var input = div.getElementsByTagName("input");
        var select = div.getElementsByTagName("select");
        for (var i = 0; i < input.length; i++) {
            json += input[i].name + ":'" + input[i].value + "',";
        }
        for (var j = 0; j < select.length; j++) {
            json += select[j].name + ":'" + select[j].value + "',";
        }
    }
    catch (error) { console.log(error); }
    json = json.substring(0, json.length - 1) + "}";
    //console.log(JSON.stringify(json));
    $.ajax({
        url: "/Home/PList",
        data: encodeURI(json),
        type: "post",
        cache: false,
        timeout: 8000,
        dataType: "json",
        ContentType: "application/json; charset=utf-8",
        error: function () { },
        beforeSend: function () { },
        success: function (data) {
            //console.log(data);
            if (data.result == 200) {
                $.each(data.data.list, function (index, content) {
                    $("#list" + number).append("<li><a href=\"/Home/Detail?gid=" + content.Gid + "\"><p><img src=\"/uploadfiles/product/" + content.Picture + "\" /></p><p class=\"title\">" + content.Prefix + content.Name + "</p><p class=\"price\">￥ " + content.Price + "/" + content.Company + "</p></a></li>");
                });
            }
        },
    });
}