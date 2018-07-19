//获取要请求的控制器
var controller = window.location.pathname + "Data";
//第一个参数"mescroll"对应上面布局结构div的id
var scroll = new MeScroll("scroll", {
    down: {
        callback: function (mescroll) {
            mescroll.resetUpScroll();
        } //下拉刷新的回调,别写成downCallback(),多了括号就自动执行方法了
    },
    up: {
        callback: function (page, mescroll) { getList(page) }, //上拉加载的回调
        isBounce: false, //如果您的项目是在iOS的微信,QQ,Safari等浏览器访问的,建议配置此项.解析(必读)
        //toTop: { //配置回到顶部按钮
        //    src: "/images/top.png", //默认滚动到1000px显示,可配置offset修改
        //    offset: 1000,
        //    warpClass: "mescroll-totop",
        //    showClass: "mescroll-fade-in",
        //    hideClass: "mescroll-fade-out",
        //    duration: 300,
        //    supportTap: false
        //},
        empty: {
            warpId: "dataList",
            icon: null,
            tip: "暂无相关数据~",
            btntext: "",
            btnClick: null,
            supportTap: false
        },
        htmlLoading: '<p class="upwarp-progress mescroll-rotate"></p><p class="upwarp-tip">加载中..</p>'
    }
});
function getList(page) {
    var json = "{";
    json += "pageindex:" + page.num;
    json += ",pagesize:" + page.size;
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
        url: controller,
        data: encodeURI(json),
        type: "post",
        cache: false,
        timeout: 8000,
        dataType: "json",
        ContentType: "application/json; charset=utf-8",
        error: function () { scroll.endErr(); },
        beforeSend: function () {
            //layer.open({ type: 2, shade: false });
        },
        success: function (data) {
            //console.log(controller);
            //console.log(data);
            //渲染数据
            var html = template('tpl', data.data);
            if (page.num == 1) {
                document.getElementById('dataList').innerHTML = html;
            }
            else {
                document.getElementById('dataList').innerHTML += html;
            }
            scroll.endBySize(data.data.list.length, data.data.count);
            //layer.closeAll();
        },
    });
}
function scrollSearch() {
    //重置列表数据
    scroll.resetUpScroll();
    //隐藏回到顶部按钮
    scroll.hideTopBtn();
}