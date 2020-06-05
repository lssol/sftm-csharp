#%%
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from os import listdir
from os.path import isfile, join
import json

#%%
folder = 'results'
files = [f for f in listdir(folder) if isfile(join(folder, f))]
files.sort(reverse=True)

final_path = join(folder, files[0])
matchings = []
with open(final_path) as fp:
    line = fp.readline()
    while line:
        j = json.loads(line)
        matchings.append(j)
        line = fp.readline()
df = pd.DataFrame(matchings)
df = df[df['mutationPercentage'] < 50]
df.describe()
#%%
sns.scatterplot(x="mutationPercentage", y="success", hue="label", data=df)
#%%
sns.scatterplot(x="total", y="success", hue="label", data=df)

#%%
sns.scatterplot(x="total", y="computationTime", hue="label", data=df)
#%%
sns.scatterplot(x="total", y="computationTime", data=df, hue='label')
#%%
sns.lmplot(x="total", y="computationTime", data=df[df['label'] == 'SFTM'], fit_reg=True)
# %%
df[df['label'] == 'SFTM']['computationTime'].hist()