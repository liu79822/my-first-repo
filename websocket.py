APPID = '你的app id'
APISecret = '你的api secret'
APIKey = '你的api key'

from datetime import datetime
from time import mktime
from wsgiref.handlers import format_date_time

cur_time = datetime.now()
date = format_date_time(mktime(cur_time.timetuple()))

print("date = ",date)

tmp = "host: " + "spark-api.xf-yun.com" + "\n"
tmp += "date: " + date + "\n"
tmp += "GET " + "/v3.1/chat" + " HTTP/1.1"

import hmac
import hashlib

tmp_sha = hmac.new(APISecret.encode('utf-8'), tmp.encode('utf-8'),digestmod=hashlib.sha256).digest()

import base64
signature = base64.b64encode(tmp_sha).decode(encoding='utf-8')

authorization_origin = f'api_key="{APIKey}", algorithm="hmac-sha256", headers="host date request-line", signature="{signature}"'

authorization = base64.b64encode(authorization_origin.encode('utf-8')).decode(encoding='utf-8')

print("authorization =",authorization)

from urllib.parse import urlencode

v = {
		"authorization": authorization, # 上方鉴权生成的authorization
        "date": date,  # 步骤1生成的date
    	"host": "spark-api.xf-yun.com" # 请求的主机名，根据具体接口替换
}
url = "wss://spark-api.xf-yun.com/v3.1/chat?" + urlencode(v)

print("url = ",url)
