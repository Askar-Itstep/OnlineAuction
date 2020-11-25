import pandas as pd
df = pd.read_json('json.txt')
df=df.head(1)
df.to_json(_, orientir='index')
