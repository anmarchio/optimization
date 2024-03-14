# -*- coding: utf-8 -*-
import numpy as np
import array as arrdir
#from scipy import *
from scipy import stats
import matplotlib
matplotlib.use('Agg') # this is required on the server, because no actual "plots" can be created and rendered
import matplotlib.pyplot as plt
import os
import sys
"""
Created on Wed Mar 11 13:53:53 2020

@author: gonzalez
"""

"""
Usage should be: python testCompare.py parentDir commitHashA commitHashB
"""
print(len(sys.argv))
for i in range(len(sys.argv)):
    print(sys.argv[i])
if(len(sys.argv)==5):
    #print("parametros:")
    #print(str(sys.argv))
    
    resultsPath0=sys.argv[2]
    
    dirResults=os.path.join(resultsPath0,"CompareResults")
    if not os.path.exists(dirResults):
        os.makedirs(dirResults)
    else: 
        #FileExistsError("Results directory already exists")
        print("Results directory already exists")
    
    dirBilderPath=os.path.join(dirResults,"Bilder")
    if not os.path.exists(dirBilderPath):
        os.makedirs(dirBilderPath)
    else:
        #FileExistsError("Bilder directory already exists")
        print("Bild directory already exists")
    version1=sys.argv[3]
    version2=sys.argv[4]
    listVersions=[]
    listActualVersions=[]
    #datenPath=os.path.abspath(os.path.join("Daten")) # WICHTIG: Das Verzeichnis Daten sollte in der selbe Verzeichnis wie der Skript testCompare.py liegen.
    datenPath=sys.argv[1] 
    resultBilderPath=os.path.join(dirResults,"Bilder")
    
    
    listVersions=os.listdir(datenPath) 
    pathVersions=[]
    listMean=[]
    listStd=[]

    for i in range(len(listVersions)):
        if(listVersions[i] == version1 or listVersions[i]==version2):
            pathVersions.insert(i,os.path.join(datenPath, listVersions[i])) 
            listActualVersions.insert(i,listVersions[i])
    resultsPath=os.path.join(dirResults,"compareResults_verion_"+str(version1)+"_mit_"+str(version2)+".txt")
    #resultsPath=os.path.abspath(os.path.join("CompareResults","compareResults_version_"+str(version1)+"_mit_"+str(version2)+".txt"))
    
    lBilder=[] #listBilder
    lBilder2=[]
    lBilder3=[]
    lBilder4=[] #hier sind die Bilder die vollständig sind
    lNBilder=[]
    lPathBilder=[]
    lPathBilder2=[]
    lPathBilderOverview=[]
    overviewP=[]
    overviewP2=[]
    for w in range(len(listActualVersions)):
        lBilder.insert(w,os.listdir(pathVersions[w]))
    
    for b in range(len(lBilder)-1):
        for f in range(len(lBilder[0])):
            if(lBilder[b][f]==lBilder[b+1][f]):
                lBilder2.append(lBilder[b][f])
    
    
    for h in range(len(listActualVersions)):
        
        for z in range(len(lBilder2)):
            lPathBilder.insert(z,os.path.join(pathVersions[h],lBilder2[z]))
            oP=os.path.abspath(os.path.join(lPathBilder[z],"overview.txt"))
            if(os.path.exists(oP)==True):
                lBilder3.append(lBilder2[z])
                #lPathBilder2.append(oP)
    for l in range(len(lBilder2)):
        if(lBilder3.count(lBilder2[l])==len(listActualVersions)):
            lBilder4.append(lBilder2[l])
        else:
            lNBilder.append(lBilder2[l])
            
    print("")
    print("Es fehlt folgende Daten: ")
    print(lNBilder)
    
    for t in range(len(listActualVersions)):
        for g in range(len(lBilder4)):
            lPathBilder2.insert(g,os.path.join(pathVersions[t],lBilder4[g]))
            oP2=os.path.abspath(os.path.join(lPathBilder2[g],"overview.txt"))
            lPathBilderOverview.append(oP2)

    listBestFitnessItVersion=np.zeros((len(listActualVersions),(len(lBilder4)*5)))#es gibt nur Overview für 3 Bilder also 3*5=15 spalten 
    #listAllFitness=np.zeros((202,(len(lBilder4)*len(listActualVersions)*5)))
    fig = plt.figure(figsize=(len(lBilder4)*5,len(listActualVersions)*3.5))
    fig.subplots_adjust(hspace=0.4,wspace=0.4)
    with open(resultsPath,'w') as file:
            
        for i in range(len(listActualVersions)):
            
            print("")
            print("Actual Version: ")
            print(listActualVersions[i])
            file.write(os.linesep + "Version: " + listActualVersions[i]+os.linesep)
            listPathBilder=[]
            bestFitnessValues=[]
            
            
            for s in range(0,len(lBilder4)):
                listPathBilder.insert(s,os.path.join(pathVersions[i], lBilder4[s]))
                    
            #listBestFitnessItVersion=np.zeros((len(listActualVersions),len(listBilder)*5)) #wenn es Overview für alle gibt 
            listFitnessIt=[] #hier werden 200 fitnessValues pro jede Iteration gespeichert
          
            lenListBilder4=len(lBilder4)
            for j in range(0,len(lBilder4)): #es gibt kein Overview für carbon_convert in 9f84163d sonst ab 0
                
                overviewPath=os.path.abspath(os.path.join(listPathBilder[j], "overview.txt"))
                obj=open(overviewPath,"r")
                gesamtStringArray=[]
                difIt=[]
                fitIt=np.zeros((202,len(listActualVersions))) #202generaionen
                #fig = plt.figure('Version' + str(listActualVersions[i]) + 'Bild: ' + str(lBilder4[j]) ,figsize=(10,5))
                m=(i*lenListBilder4)+j
                axs=fig.add_subplot(len(listActualVersions),lenListBilder4,m+1)
                #plt.figure(j+4+i)
                legend=0
                legend2=0
                for k in range(5):# Anzahl Iterationen
                    gesamtStringArray.insert(k,obj.readline())
                    charLine=gesamtStringArray[k].split()
                    floatValue=float(charLine[4])
                    bestFitnessValues.insert(k,floatValue)
                    #listFitnessIt.insert(k*j,floatValue)
                    dif=[]
                    fb=[]
                    #200fitness Values pro jede Iteration
                    analyzerPath=os.path.abspath(os.path.join(listPathBilder[j], "Analyzer",str(k), "BestIndividualFit.txt" ))
                    objBestFit=open(analyzerPath,"r")
                    readFitness=[]
                    #fitIt=[]
                    for l in range(202):
                        readFitness.insert(l,objBestFit.readline())      
                    for m in range(1,202):
                        temp=list(readFitness[m].split()[0])
                        fit=""
                        for n in range(len(temp)):
                            if(temp[n]=='.'):
                                fit=fit+temp[n-1]
                                indexPunkt=temp.index('.')
                                for t in range(indexPunkt, len(temp)):
                                    fit=fit+temp[t]
                        if(fit==''):
                            fit=0.0
                        else:
                            fit=float(fit)
                        dif1=1.0-fit
                        dif.insert(m-1,dif1)
                        #dif.insert(202*k+m-1,dif1)
                        listFitnessIt.insert(202*k+m-1,fit)
                        fitIt[m][i]=fit
                        fb.insert(m-1,fit)
                        #fitIt[i].insert(m-1,fit)
                    
                        difMean=np.average(dif)
                        difIt.append(difMean)
                        #print("fitIt: " + str(k) + "Version:" + str(listVersions[0]) + "Bild" + str(listBilder[0][j]))
                        sizefb=len(fb)
                        
                    axs.plot(np.linspace(0,sizefb,sizefb),fb,label='Iteration: ' + str(k))
                    #plt.title("Version: "+ str(listActualVersions[i])+" Image: " + str(lBilder4[j]))
                    axs.set_title("Version: "+ str(listActualVersions[i])+" Image: " + str(lBilder4[j]))
                    axs.set_xlabel("Generationen")
                    axs.set_ylabel("Fitness")
                    axs.legend()
                    
                    #fig.add_subplot(len(listActualVersions),lenListBilder,j)
                    #pathNewBilder=os.path.abspath(os.path.join("gonzalez","Documents","compareResults","Bilder"))
                    #plt.savefig(os.path.join(pathNewBilder,"Version_"+ str(listActualVersions[i])+ "__Bild_"+ str(listBilder[0][j])))
                    
                    objBestFit.close()
                
                  
            listBestFitnessItVersion[i]=bestFitnessValues
            #print(bestFitnessValues)
            listMean.insert(i,np.mean(bestFitnessValues)) #index 0=mean of version 09ed8a63
            listStd.insert(i,np.std(bestFitnessValues))
            #print("Fitness values: ")
            #print(fitnessValues)
            print("Mean: ")
            print(listMean[i])
            file.write("Mean: " + str(listMean[i]) + os.linesep)
            #print("Mean2: ")
            #print(np.average(listFitnessIt))
            print("Sdt: ")
            print(listStd[i])
            file.write("Std: " + str(listStd[i]) + os.linesep)
            
    
            print("Durchschnitt der Differenz insgesamt: ")
            print(np.average(difIt))
            file.write("Durchschnitt der Differenr insgesamt: " + str(np.average(difIt)) + os.linesep)
            print("Min Fitness: ")
            print(np.min(listFitnessIt))
            file.write("Min Fitness: " + str(np.min(listFitnessIt)) + os.linesep)
            print("Max Fitness: ")
            print(np.max(listFitnessIt))
            file.write("Max Fitness: " + str(np.max(listFitnessIt)) + os.linesep)
            
        
            
            obj.close()
        
        plt.savefig(os.path.join(resultBilderPath,"Version_"+ str(listActualVersions[i])+ "__Bild_"+ str(lBilder4[j])))
            
        
        f=plt.figure(figsize=(10,len(listActualVersions)*8))
        #plt.title("Normal distribution and Histogram of best fitness values" )
        f.subplots_adjust(hspace=0.4,wspace=0.4)
        #f=plt.figure("Normal distribution and Histogram of best fitness values" )   
    
        #plt.title("Normal distribution and Histogram of best fitness values")   
        for i in range(len(listActualVersions)):
            normal=stats.norm(loc=listMean[i],scale=listStd[i])
            x=np.linspace(normal.ppf(0.001),normal.ppf(0.999),100)
            normalDichte=normal.pdf(x)
            axs2=f.add_subplot(len(listActualVersions),1,(i+1))
            plt.plot(x,normalDichte)
            
            for h in range(len(lBilder4)):
                fit=[]
                for w in range(5):
                    m=(h*5)+w
                    fit.append(listBestFitnessItVersion[i][m])
                #meanFit=np.mean(fit)
                #stdFit=np.std(fit)
                #normal0=stats.norm(loc=meanFit,scale=stdFit)
                #x=np.linspace(normal0.ppf(0.001),normal0.ppf(0.999),100)
                #normalDichte=normal0.pdf(x)
                plt.hist(fit,label=lBilder4[h])
                plt.title( "Normal distribution and Histogram of best fitness values (Version: "+str(listActualVersions[i]) +")" )
                plt.xlabel("Fitness")
                plt.ylabel("Frequency")
                plt.legend()
            
        plt.savefig(os.path.join(resultBilderPath,"Normal distribution" + "_version"+ str(listActualVersions[i])))
        
else:
    print("First parameter should be the data directory, second parameter should be the results directory,third and fourth the commit hashes that are to be compared, i.e.: <data_dir> <results_dir> <hashA> <hashB>")  
  
