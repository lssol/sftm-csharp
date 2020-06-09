#%%
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from os import listdir
from os.path import isfile, join
import json
import numpy as np
from pymongo import MongoClient

#%%
uri = 'mongodb://wehave_prod%40service:AX3ohnia@datalakestar.amarislab.com:27018/?authMechanism=PLAIN&ssl=true'
db = MongoClient(uri).get_database('locatorBenchmark')
collection = db['VLDB_Mutation_SimulationResults']

results = list(collection.find())
df = pd.DataFrame(results)

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

#%%
sns.lmplot(x="total", y="computationTime", data=df[df['label'] == 'RTED'], order=3)
