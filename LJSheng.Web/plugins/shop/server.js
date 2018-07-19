/*
    功能：查看本地数据中是否含有指定的对象（商品），根据id
    参数：id：商品的标识
 */
function checkObjByGid(id) {
    var jsonStr = cookieObj.get("shopdatas");
    var jsonObj = JSON.parse(jsonStr);
    var isExist = false;
    for (var i = 0, len = jsonObj.length; i < len; i++) {
        if (jsonObj[i].gid == id) {
            isExist = true;
            break;
        }
    }
    return isExist; //return false;
}

/*
    功能：更新本地数据
    参数：arr    数组对象
    返回一个值：最新的本地转换后的数组对象
 * */
function updateData(arr) {
    var jsonStr = JSON.stringify(arr);
    cookieObj.set({
        name: "shopdatas",
        value: jsonStr
    });
    jsonStr = cookieObj.get("shopdatas");
    return JSON.parse(jsonStr);
}

/*
    获取商品的总数量
    返回：数字
 */
function getTotalCount() {
    /*循环遍历数组，获取每一个对象中的pCount值相加总和*/
    var totalCount = 0; //默认为0
    var jsonStr = cookieObj.get("shopdatas");
    var listObj = JSON.parse(jsonStr);
    for (var i = 0, len = listObj.length; i < len; i++) {
        totalCount = listObj[i].pCount + totalCount;
    }
    return totalCount;
}

/*
    获取商品的总价格
    返回：数字
 */
function getTotalPrice() {
    var totalPrice = 0; //默认为0
    var jsonStr = cookieObj.get("shopdatas");
    var listObj = JSON.parse(jsonStr);
    for (var i = 0, len = listObj.length; i < len; i++) {
        totalPrice += Number(listObj[i].pCount) * Number(listObj[i].price);
    }
    return totalPrice;
}


/*
    更新本地数据根据gid
    id:商品的标识
 */
function updateObjById(id, num) {
    var jsonStr = cookieObj.get("shopdatas");
    var listObj = JSON.parse(jsonStr);
    for (var i = 0, len = listObj.length; i < len; i++) {
        if (listObj[i].gid == id) {
            listObj[i].pCount = listObj[i].pCount + num;
            break;
        }
    }
    return updateData(listObj)
}

/*
    获取本地数据
    返回 数组对象
 * */
function getAllData() {
    var jsonStr = cookieObj.get("shopdatas");
    var listObj = JSON.parse(jsonStr);
    return listObj;
}

function deleteObjByGid(id) {
    var lisObj = getAllData();
    for (var i = 0, len = lisObj.length; i < len; i++) {
        if (lisObj[i].gid == id) {
            lisObj.splice(i, 1);
            break;
        }
    }
    updateData(lisObj);
    return lisObj;
}