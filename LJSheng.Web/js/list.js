//获取要请求的控制器
var controller = window.location.pathname.split('/');
controller = controller[controller.length - 1].replace("List", "").replace("list", "");
//分页
var pages = '<li class="input-group">';
pages += '<select class="form-control" id="pagesize" style="width:8rem;">';
pages += '<option value="1"> 1  条/页</option>';
pages += '<option value="5"> 5  条/页</option>';
pages += '<option value="10" selected>10 条/页</option>';
pages += '<option value="15">15 条/页</option>';
pages += '<option value="20">20 条/页</option>';
pages += '<option value="50">50 条/页</option>';
pages += '</select>';
pages += '</li>';
pages += '<li><a href="#">到第</a></li>';
pages += '<li class="input-group"><input type="text" id="topage" class="form-control" style="width:3.5rem;"></li>';
pages += '<li><a href="#">页</a></li>';
pages += '<li class="input-group"><button class="btn" type="button" onclick="GetList(0);">确定</button></li>';
pages += '<li class="input-group"> 共 <em id="page"></em> 页,<em id="count"></em> 条数据</li>';
pages += '<li class="input-group" id="other"></li>';
$("#pages").html(pages);
GetList(1);
function GetList(pageindex) {
    pageindex = pageindex === 0 ? $("#topage").val() : pageindex;
    var pagesize = $("#pagesize").val();
    var json = "{";
    json += "pageindex:" + pageindex;
    json += ",pagesize:" + pagesize;
    try {
        var div = document.getElementById("search");
        var input = div.getElementsByTagName("input");
        var select = div.getElementsByTagName("select");
        for (var i = 0; i < input.length; i++) {
            json += "," + input[i].name + ":'" + input[i].value + "'";
        }
        for (var j = 0; j < select.length; j++) {
            json += "," + select[j].name + ":'" + select[j].value + "'";
        }
    }
    catch (error) { console.log(error); }
    json += "}";
    //console.log(json);
    //重新加载数据
    $.ajax({
        url: "/LJSheng/" + controller,
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
            //渲染数据
            var html = template('tpl', data.data);
            document.getElementById('tbody').innerHTML = html;
            //计算共多少页
            var page = Math.ceil(data.data.count / pagesize);
            if (page !== 0) {
                $("#pages").show();
                $("#pager").show();
                //上一页状态
                var previous = "";
                //下一页状态
                var next = "";
                if (data.data.pageindex === 1) {
                    previous = "disabled";
                }
                if (data.data.pageindex === page) {
                    next = "disabled";
                }
                //上一页num
                var previousnum = data.data.pageindex === 1 ? 1 : data.data.pageindex - 1;
                //分页html
                var pager = '<li><a href="javascript:GetList(1);" ' + previous + '>首页</a></li>';
                pager += '<li class="previous"><a href="javascript:GetList(' + previousnum + ');" ' + previous + '>上一页</a></li>';
                //开始页数
                var start = data.data.pageindex - 3 < 4 ? 1 : data.data.pageindex - 3;
                //结束页数
                var end = start + 7;
                if (end > page) {
                    end = page;
                }
                for (var i = start; i <= end; i++) {
                    if (data.data.pageindex === i) {
                        pager += '<li class="active">';
                    }
                    else {
                        pager += '<li>';
                    }
                    pager += '<a href="javascript:GetList(' + i + ');">' + i + '</a></li>';
                }
                //下一页num
                var nextnum = data.data.pageindex === page ? page : data.data.pageindex + 1;
                pager += '<li class="next"><a href="javascript:GetList(' + nextnum + ');" ' + next + '>下一页</a></li>';
                pager += '<li><a href="javascript:GetList(' + page + ');" ' + next + '>末页</a></li>';
                $("#topage").val(data.data.pageindex);
                $("#page").html(page);
                $("#count").html(data.data.count);
                $("#pager").html(pager);
                $("#other").html(data.data.other);
            }
            else
            {
                $("#pages").hide();
                $("#pager").hide();
            }
        },
    });
}

function PWD(gid) {
    if (window.confirm('你确定要重设密码吗？')) {
        $.ajax({
            url: "/LJSheng/" + controller + "pwd",
            data: "Gid=" + gid + "&cache=" + Math.random(),
            type: "post",
            cache: false,
            timeout: 8000,
            dataType: "json",
            ContentType: "application/json; charset=utf-8",
            error: function () { new $.zui.Messager('请求超时!').show(); },
            beforeSend: function () { },
            success: function (data) {
                Messager(data.data, data.result);
            },
        });
        return true;
    } else {
        return false;
    }
}

function Delete(gid) {
    if (window.confirm('你确定要删除吗？')) {
        $.ajax({
            url: "/LJSheng/" + controller + "delete",
            data: "Gid=" + gid + "&cache=" + Math.random(),
            type: "post",
            cache: false,
            timeout: 8000,
            dataType: "json",
            ContentType: "application/json; charset=utf-8",
            error: function () { new $.zui.Messager('请求超时!').show(); },
            beforeSend: function () { },
            success: function (data) {
                if (data.result === 200) {
                    $("#" + gid).hide(5);
                }
                Messager(data.data, data.result);
            },
        });
        return true;
    } else {
        return false;
    }
}

function Show(gid, url) {
    url = url === undefined ? "/LJSheng/" + controller + "AU?gid=" + gid : "/LJSheng/" + url;
    var myModalTrigger = new $.zui.ModalTrigger({
        type: 'iframe',
        position: 'center',
        moveable: true,
        title: gid,
        width: '1280',
        height: '720',
        backdrop: 'static',
        url: url
    });
    myModalTrigger.show();
}

function Messager(msg, result)
{
    new $.zui.Messager(msg, {
        icon: result === 200 ? 'smile' :'frown',
        placement: 'center'
    }).show();
}