"""
Export simple 32 channel probe interface file.

This script is used to generate a simple probe interface for use with a HeadstageRhs2116, and is opened by the Bonsai GUI for the StimulusTrigger to aid in determining channel locations

"""

import numpy as np
from probeinterface import Probe, ProbeGroup
from probeinterface import write_probeinterface
from probeinterface.plotting import plot_probe, plot_probe_group

num_channels = 16
positions = np.zeros((num_channels, 2))
contact_idsA = [ 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15]
device_idsA =  [ 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15]

for i in range(num_channels):
    x = i + 1
    y = 3
    positions[i] = x, y

probeA = Probe(ndim=2, si_units='mm')
polygonA = [(0.5, 2.5), (num_channels + 0.5, 2.5), (num_channels + 0.5, 3.5), (0.5, 3.5), (0.5, 2.5)]

probeA.set_contacts(positions=positions, shapes='circle', shape_params={'radius': 0.3})
probeA.set_contact_ids(contact_ids=contact_idsA)
probeA.set_device_channel_indices(device_idsA)
probeA.set_planar_contour(polygonA)

positions[:, 1] -= 2
polygonB = [(0.5, 0.5), (num_channels + 0.5, 0.5), (num_channels + 0.5, 1.5), (0.5, 1.5), (0.5, 0.5)]

contact_idsB = [16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31]
device_idsB =  [16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31]

probeB = Probe(ndim=2, si_units='mm')

probeB.set_contacts(positions=positions, shapes='circle', shape_params={'radius': 0.3})
probeB.set_planar_contour(polygonB)
probeB.set_contact_ids(contact_ids=contact_idsB)
probeB.set_device_channel_indices(device_idsB)

probegroup = ProbeGroup()
probegroup.add_probe(probeA)
probegroup.add_probe(probeB)

# To plot the probe group, uncomment the following lines

#import matplotlib.pyplot as plt
#plot_probe_group(probegroup, same_axes=True, with_contact_id=True, with_device_index=True)
#plt.show()

write_probeinterface('simple_rhs2116_probe_interface.json', probegroup)