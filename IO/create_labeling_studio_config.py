from tkinter import filedialog
import random
import os


def create_config():
    # Select csv file and generate config from the first line.
    file_path = filedialog.askopenfilename(filetypes=[("CSV File", "*.csv")], initialdir=os.curdir)
    if file_path == '':
        print('No file selected! Try again.')
        return
    with open(file_path, 'r') as f:
        header = f.readline()[:-1]
        columns = header.split(',')
        x_value = columns[0]
        channels = create_channels(columns[1:])
        config = f'''
<View>
    <Header value="Time Series from CSV" style="font-weight: normal">
    </Header>
    <TimeSeriesLabels name="label" toName="ts">
      <Label value="anomaly" background="#55f">
      </Label>
    </TimeSeriesLabels>
    <TimeSeries name="ts" timeColumn="{x_value}" valueType="url" value="$csv" sep="," overviewChannels="velocity">
        {channels}
    </TimeSeries>
</View>
'''
        print(config)
        dest_file = os.path.join(os.curdir, 'label_studio_config.xml')
        if os.path.exists(dest_file):
            os.remove(dest_file)
        with open(dest_file, 'x') as config_file:
            config_file.write(config)


def create_channels(columns:[]):
    channels = ''
    indent = 2
    for c in columns:
        channels += create_channel(c, indent=indent)
    return channels[indent:-1]


def create_channel(column, indent=4):
    indent = '\t' * indent
    color = "%06x" % random.randint(0, 0xFFFFFF)
    channel = f'{indent}<Channel column="{column}" strokeColor="#{color}"> </Channel>\n'
    return channel


if __name__ == "__main__":
    create_config()
