# -*- coding: utf-8 -*-
"""
Created on Fri May 15 11:39:55 2020

@author: gonzalez
"""

import argparse
import os
from PIL import Image,ImageOps  
import numpy as np
from skimage.io import imread, imsave
import skimage.io as io

from matplotlib import pyplot
from skimage import color
from skimage import data
from skimage.util import crop, pad
from skimage.util import pad

import matplotlib.pylab as plt

parser=argparse.ArgumentParser(description='Transform images')

parser.add_argument('data_directory',
                    help='directory containing images in images/labels format')

parser.add_argument('transform_results_directory',
                    help='result directory to place transformed images in')

parser.add_argument('--split', 
                    type=int,
                    default=0,
                    help='Crop a big picture into several small size pictures')

#1=True and 0=False
parser.add_argument('--cropRegions',
                    default=0, 
                    help='Crop around regions of interest. Parameters expected: 0 or 1')

parser.add_argument('--resize',
                    default=0,
                    help='resize an image with factor n')

args=parser.parse_args()


train_data=args.data_directory

splitFactor=args.split
resizeFactor=args.resize
cropParameter=args.cropRegions
dirResults=args.transform_results_directory

funktionen=[]
if(splitFactor!=0):
    funktionen.append("split")
if(cropParameter!=0):
    funktionen.append("crop")
if(resizeFactor!=0):
    funktionen.append("resize")
print(funktionen)


dirGesamtTrainImages=[]
dirGesamtTrailLabels=[]
dirGesamtBilder=[]
dirGesamtBilder.append(os.path.join(train_data,"images"))
dirGesamtBilder.append(os.path.join(train_data,"labels"))


listNameImages=[]


for i in range(len(dirGesamtBilder)):
    # avoid .db files that microsoft likes to put everywhere for thumbnails
    listNameImages.append([x for x in os.listdir(dirGesamtBilder[i]) if '.db' not in x])
    

#Images und Labels sollen derselbe Name und derselbe size haben 
def matchNames_size():
    temp=False
    for i in range(len(listNameImages)):
        if(i%2==0):
            temp=False
            if(len(listNameImages[i])==len(listNameImages[i+1])):
                temp=True
                for k in range(len(listNameImages[i])):
                    namestr=os.path.splitext(listNameImages[i][k])
                    namestr2=os.path.splitext(listNameImages[i+1][k])
                    dirImg=os.path.join(dirGesamtBilder[i],listNameImages[i][k])
                    dirImg2=os.path.join(dirGesamtBilder[i+1],listNameImages[i+1][k])
                    img=Image.open(dirImg)
                    img2=Image.open(dirImg2)
                    if(namestr[0]==namestr2[0] and temp==True and img.size==img2.size):
                        temp=True
                    else: temp=False
                    img.close()
                    img2.close()
       
    if(temp==True):
        print(temp)
        return True                
    else:
        print("Names or size of images are different")
    
    
#os.path.abspath entfernen
#para cada imagen no es necesario un directorio 
dirResultsImage=os.path.join(dirResults, "images")
dirResultsLabels=os.path.join(dirResults,"labels")

widthImages=[]
heightImages=[]

#jeder Bild wird in kleinere Bilder geteilt. SplitFactor=Anzahl von kleinere Bilder
#d.h wenn splitFactor=4 wird jede Bild in 4 kleinere Bilder geteilt 
def split(img,i,nameBild):
    #print(img.height, img.width)
    nameBild2=os.path.splitext(nameBild)
    nameBild3=nameBild2[0]
    temp2=nameBild2[1]
    width=img.shape[1]
    temp=width/splitFactor
    if(i==0):
        #print("Image")
        for k in range(splitFactor):
            j=width-((k+1)*temp)
            #if(len(img.shape)==2):
            cropImage=crop(img,((0,0),(k*temp,j)),copy=False)
            #print(cropImage.shape)
            dirResultsImages=os.path.abspath(os.path.join(dirTrainImagesResults, nameBild3+"_split_"+str(k)+temp2))
            imsave(dirResultsImages,cropImage)
            
    if(i==1):
        #print("Image")
        for k in range(splitFactor):
            j=width-((k+1)*temp)
            #if(len(img.shape)==2):
            cropImage=crop(img,((0,0),(k*temp,j)),copy=False)
            #print(cropImage.shape)
            dirResultsImages=os.path.abspath(os.path.join(dirTrainLabelsResults, nameBild3+"_split_"+str(k)+temp2))
            imsave(dirResultsImages,cropImage)

#jeder Bild wird um ein Factor n resize. Z.B: ein Bild mit width=100 und resizeFator=0.8 wird eine neue width=80 haben.
def resize(imgDir,factor,i,nameBild):
    nameBild2=list(nameBild)
    nameBild3=""
    #print(nameBild2)
    for m in range(len(nameBild2)-4):
        nameBild3=nameBild3+nameBild[m]
    
    img=Image.open(imgDir)
    width=int(round(img.size[0]*float(factor),0))
    height=int(round(img.size[1]*float(factor),0))
    reImage=img.resize((width,height),Image.NEAREST)
    
    if(i!=0 and i!=1 and i!=2 and i!=3 ):
        return reImage
    if(i==0):                
        dirResultsImages=os.path.join(dirTrainImagesResults, "resize_"+nameBild)
        reImage.save(dirResultsImages)
        
    if(i==1):           
        dirResultsImages=os.path.join(dirTrainLabelsResults,"resize_" +nameBild)
        reImage.save(dirResultsImages)

#Crop around regions of interest (such as the smallest rectangle containing a region of interest)
def cropImg(dirImg,nameBild,dirCropRes):
    
    img=Image.open(dirImg[0]).convert('L')
    #label=Image.open(dirImg[1])
    label=Image.open(dirImg[1]).convert('L')
    labelW=label.width
    labelH=label.height
    labelArray=np.asarray(label)
    labelIndexI=[]
    labelIndexJ=[]
    
    for i in range(labelArray.shape[0]):
        for j in range(labelArray.shape[1]):
            if(labelArray[i][j]!=0):
                labelIndexI.append(i)
                labelIndexJ.append(j)
                    
    labelIarray=np.asarray(labelIndexI)
    labelJarray=np.asarray(labelIndexJ)
    minIndexI=np.amin(labelIarray) 
    maxIndexI=np.amax(labelIarray) #max index for height 
    minIndexJ=np.amin(labelJarray)
    maxIndexJ=np.amax(labelJarray) #max index for width
    k=labelW-maxIndexJ
    m=labelH-maxIndexI
    cropLabel=crop(label,((minIndexI,m),(minIndexJ,k)),copy=False)
    cropImage=crop(img,((minIndexI,m),(minIndexJ,k)),copy=False)
    imsave(dirCropRes[0],cropImage)
    imsave(dirCropRes[1],cropLabel)
    
if(matchNames_size()==1):
    #print(listNameImages)
    #namestr=list(listNameImages[0][0])
    #nameBild=""
    dirImg=[]

    #dirResultName=os.path.join(dirResults,nameBild)
    dirTrainImagesResults=os.path.join(dirResults,"images")
    dirTrainLabelsResults=os.path.join(dirResults,"labels")


    for d in [dirResults, dirTrainImagesResults,
              dirTrainLabelsResults]:
        try: os.makedirs(d)
        except FileExistsError: pass

    cropImgI=[]
    cropImgName=[]
    for i in range(len(dirGesamtBilder)):
        for j in range(len(listNameImages[i])):        
            dirImage=os.path.join(dirGesamtBilder[i],listNameImages[i][j])
            nameBild=listNameImages[i][j]
            if(cropParameter!=0):
                cropImgI.append(dirImage)
                cropImgName.append(nameBild)
                w=len(listNameImages[0])*2
                v=len(listNameImages[0])*2
                if(len(cropImgI)%w==0 or len(cropImgI)%(v+w)==0):
                    for t in range(len(listNameImages[i])):
                        if(len(cropImgI)==2*len(listNameImages[0])):                            
                            n=len(listNameImages[i])+t
                            dirImg.append(cropImgI[t])
                            dirImg.append(cropImgI[n])
                            cropImgName2=[]
                            cropImgName2.append(cropImgName[t])
                            cropImgName2.append(cropImgName[n])
                            if(len(dirImg)%2==0):
                                nameBild1=os.path.splitext(cropImgName2[0])
                                nameBild2=os.path.splitext(cropImgName2[1])
                                nameBild1_1=nameBild1[0]
                                nameBild2_2=nameBild2[0]
                                temp1=nameBild1[1]
                                temp2=nameBild2[1]              
                            
                                dirCropRes=[]
                                if(resizeFactor!=0):
                                    dirResultsCropI=os.path.abspath(os.path.join(dirTrainImagesResults, nameBild1_1+"_"+"crop"+"_resize"+temp2))
                                    dirResultsCropL=os.path.abspath(os.path.join(dirTrainLabelsResults, nameBild2_2+"_"+"crop"+"_resize"+temp2))
                                    dirCropRes.append(dirResultsCropI)
                                    dirCropRes.append(dirResultsCropL)
                                else:
                                    dirResultsCropI=os.path.abspath(os.path.join(dirTrainImagesResults, nameBild1_1+"_"+"crop"+temp2))
                                    dirResultsCropL=os.path.abspath(os.path.join(dirTrainLabelsResults, nameBild2_2+"_"+"crop"+temp2))
                                    dirCropRes.append(dirResultsCropI)
                                    dirCropRes.append(dirResultsCropL)
                                
                                cropImg(dirImg,nameBild,dirCropRes)
                                dirImg=[]

            if(resizeFactor!=0 and cropParameter==0 and splitFactor==0): 
                resize(dirImage,resizeFactor,i,nameBild)
                
            if(splitFactor!=0):
                img = Image.open(dirImage).convert('L')
                width=img.width
                        
                if(width%splitFactor==0):
                    split(img,i,nameBild)    
                else:
                    temp=0
                    while(width%splitFactor!=0):
                        width=width+temp
                        temp=temp+1
                        padImage=pad(img,((0,0),(0,temp)),'constant')
                    split(padImage,i,nameBild)
                img.close()
else:
    print("In the given directory are not the correct Images")
#print(train_data)
