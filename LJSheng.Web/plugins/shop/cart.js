try {
    var listObj = getAllData();
    var table = document.getElementById("table")
    var pay = document.getElementById("pay")
    var box = document.getElementById("box")
    var allCheck = document.getElementById("allCheck");
    if (listObj.length == 0) { //购物车为空
        box.style = "";
        table.className = "hide";
        pay.style = 'display:none;';
    } else {
        box.style = 'display:none;';
        table.className = "block";
        pay.style = "";
        for (var i = 0, len = listObj.length; i < len; i++) {
            var div = document.createElement("div");
            div.setAttribute("gid", listObj[i].gid);
            div.className = "cart-item";
            //{"gid":值,"pImg":值,"pName":值,"pDesc":值,"price":值,"pCount":1},
            div.innerHTML = '<input type="checkbox" class="ck icon-check hide"  />'
                + '<span class="img"><img src="' + listObj[i].pImg + '" /></span>'
                + '<div class="flex-right">'
                + '<div class="labels">' + listObj[i].pName + '</div>'
                + '<div class="cell-row">'
                + '<span class="price">¥<b>' + listObj[i].price + '</b></span>'
                + '<div class="right-labels">'
                + '<a class="icon-plus"></a>'
                + '<input type="text" class="input-text" value="' + listObj[i].pCount + '" disabled />'
                + '<a class="icon-minus"></a>'
                + '<button class="del"> 删除</button>';
            +  '￥<span class="hide">' + listObj[i].price * listObj[i].pCount + '</span>'
                + ' </div></div></div>';
            table.appendChild(div);
        }
    }
    getTotal();
    /*
        选择列表
     */
    var cks = document.querySelectorAll(".ck");

    /*循环遍历为每一个checkbox添加一个onchange事件*/
    for (var i = 0, len = cks.length; i < len; i++) {
        cks[i].onchange = function () {
            checkAllChecked();
            getTotal();
        }
    }

    /*全选实现*/
    allCheck.onchange = function () {
        if (this.checked) {
            for (var i = 0, len = cks.length; i < len; i++) {
                cks[i].checked = true;
            }
        } else {
            for (var i = 0, len = cks.length; i < len; i++) {
                cks[i].checked = false;
            }
        }
        getTotal();
    }

    var downs = document.querySelectorAll(".icon-plus"); //一组减的按钮
    var ups = document.querySelectorAll(".icon-minus"); //一组加的按钮
    var dels = document.querySelectorAll(".del"); //一组删除按钮
    for (var i = 0, len = downs.length; i < len; i++) {
        downs[i].onclick = function () {
            var txtObj = this.nextElementSibling;//下一个兄弟节点
            var div = this.parentNode.parentNode.parentNode.parentNode;//上一个兄弟节点
            var gid = div.getAttribute("gid");
            txtObj.value = txtObj.value - 1;
            if (txtObj.value < 1) {
                txtObj.value = 1;
                updateObjById(gid, 0)
            } else {
                updateObjById(gid, -1)
            }
            //div.children[0].firstElementChild.checked = true;
            //checkAllChecked();
            //var price = div.children[4].firstElementChild.innerHTML;
            //div.children[5].firstElementChild.innerHTML = price * txtObj.value;
            getTotal();

        }

        ups[i].onclick = function () {
            var txtObj = this.previousElementSibling;//上一个兄弟节点
            var div = this.parentNode.parentNode.parentNode.parentNode;//上一个兄弟节点
            var gid = div.getAttribute("gid");
            txtObj.value = Number(txtObj.value) + 1;
            updateObjById(gid, 1)
            //div.children[0].firstElementChild.checked = true;
            //checkAllChecked()
            //var price = tr.children[4].firstElementChild.innerHTML;
            //div.children[5].firstElementChild.innerHTML = price * txtObj.value;
            getTotal();
        }

        dels[i].onclick = function () {
            var div = this.parentNode.parentNode.parentNode.parentNode;//上一个兄弟节点
            var gid = div.getAttribute("gid");
            if (confirm("确定删除？")) {
                //remove()  自杀
                div.remove();
                listObj = deleteObjByGid(gid);
            }
            console.log(listObj.length);
            if (listObj.length == 0) { //购物车为空
                box.style = "";
                table.className = "hide";
                pay.style = 'display:none;';
            } else {
                box.style = 'display:none;';
                table.className = "block";
                pay.style = "";
            }
            getTotal();
        }
    }

    /*检测是否要全选*/
    function checkAllChecked() {
        var isSelected = true; //全选是否会选中
        for (var j = 0, len = cks.length; j < len; j++) {
            if (cks[j].checked == false) {
                isSelected = false;
                break;
            }
        }
        allCheck.checked = isSelected;
    }

    /*统计数量和金额*/
    function getTotal() {
        document.getElementById("totalPrice").innerHTML = getTotalPrice();
        document.getElementById("ccount").innerHTML = getTotalCount();
    }
} catch(err){ }